// PWA registration, notification bell and Web Push subscription.
// Pairs with the /Notifications endpoints and Views/Shared/_NotificationBell.cshtml.
(function () {
  'use strict';

  const el = id => document.getElementById(id);
  const bellCount  = el('notif-bell-count');
  const list       = el('notif-list');
  const markAll    = el('notif-mark-all');
  const enableBtn  = el('notif-enable-push');
  const pushHint   = el('notif-push-hint');
  const installBtn = el('pwa-install');

  if (!list) return; // layout without the bell (e.g. login page)

  const token = () => document.querySelector('#notif-antiforgery input[name="__RequestVerificationToken"]')?.value || '';

  const post = (url, body) => fetch(url, {
    method: 'POST',
    credentials: 'same-origin',
    headers: {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'RequestVerificationToken': token()
    },
    body: body ? JSON.stringify(body) : null
  });

  // ── Bell ────────────────────────────────────────────────────────────────
  function renderFeed(data) {
    const unread = data.unread || 0;
    bellCount.textContent = unread > 99 ? '99+' : String(unread);
    bellCount.hidden = unread === 0;
    if (markAll) markAll.hidden = unread === 0;

    list.replaceChildren();
    if (!data.items || data.items.length === 0) {
      const empty = document.createElement('div');
      empty.className = 'p-4 text-center text-600';
      empty.textContent = 'No notifications yet';
      list.appendChild(empty);
      return;
    }

    for (const item of data.items) {
      const a = document.createElement('a');
      a.className = 'list-group-item list-group-item-action border-0 py-2 px-3' + (item.isRead ? '' : ' bg-soft-primary');
      a.href = item.openUrl;

      const head = document.createElement('div');
      head.className = 'd-flex justify-content-between gap-2';
      const title = document.createElement('strong');
      title.className = 'text-900 text-truncate';
      title.textContent = item.title;
      const time = document.createElement('small');
      time.className = 'text-500 text-nowrap';
      time.textContent = item.timeAgo;
      head.append(title, time);

      const body = document.createElement('div');
      body.className = 'text-700 fs--1';
      body.style.whiteSpace = 'normal';
      body.textContent = item.body;

      a.append(head, body);
      list.appendChild(a);
    }
  }

  async function refreshFeed() {
    try {
      const res = await fetch('/Notifications/Feed', { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
      if (res.ok && (res.headers.get('content-type') || '').includes('json')) renderFeed(await res.json());
    } catch { /* offline — keep last state */ }
  }

  markAll?.addEventListener('click', async e => {
    e.preventDefault();
    e.stopPropagation();
    await post('/Notifications/MarkAllRead');
    refreshFeed();
  });

  refreshFeed();
  setInterval(() => { if (document.visibilityState === 'visible') refreshFeed(); }, 60000);
  document.addEventListener('visibilitychange', () => { if (document.visibilityState === 'visible') refreshFeed(); });

  // ── Service worker + push ───────────────────────────────────────────────
  const isIos        = /iphone|ipad|ipod/i.test(navigator.userAgent) ||
                       (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
  const isStandalone = window.matchMedia('(display-mode: standalone)').matches || navigator.standalone === true;
  const pushCapable  = 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;

  function showHint(text) {
    if (!pushHint) return;
    pushHint.textContent = text;
    pushHint.hidden = !text;
  }

  function urlBase64ToUint8Array(base64) {
    const padding = '='.repeat((4 - base64.length % 4) % 4);
    const raw = atob((base64 + padding).replace(/-/g, '+').replace(/_/g, '/'));
    return Uint8Array.from([...raw].map(c => c.charCodeAt(0)));
  }

  const isEdge = /Edg\//.test(navigator.userAgent);
  const BLOCKED_HINT = isEdge
    ? 'Notifications are blocked for this site. Click the lock icon left of the address bar, open Permissions for this site, set Notifications to Allow, then reload.'
    : "Notifications are blocked. Allow them in your browser's site settings, then reload.";
  // Edge (and Chrome with "quieter messaging") hide the prompt and leave permission at "default".
  const QUIET_HINT = isEdge
    ? 'Edge hid the permission prompt. Click the bell with a line through it at the right of the address bar and choose Allow. Notifications switch on automatically.'
    : 'Your browser hid the permission prompt. Look for a notification icon in the address bar and choose Allow. Notifications switch on automatically.';

  async function sendSubscription(sub) {
    const json = sub.toJSON();
    const res = await post('/Notifications/Subscribe', { endpoint: json.endpoint, keys: json.keys });
    if (!res.ok) throw new Error('saving the subscription failed (' + res.status + ')');
  }

  const toB64 = buf => btoa(String.fromCharCode(...new Uint8Array(buf)));

  let subscribing = false;
  async function subscribe(reg) {
    if (subscribing) return;
    subscribing = true;
    try {
      const permission = Notification.permission === 'granted' ? 'granted' : await Notification.requestPermission();
      if (permission !== 'granted') {
        showHint(permission === 'denied' ? BLOCKED_HINT : QUIET_HINT);
        return;
      }
      const res = await fetch('/Notifications/VapidPublicKey', { credentials: 'same-origin' });
      if (!res.ok) throw new Error('could not load the push key (' + res.status + ')');
      const { publicKey } = await res.json();
      const key = urlBase64ToUint8Array(publicKey);

      let sub = await reg.pushManager.getSubscription();
      // A subscription made with an old key can't receive pushes signed with the current one.
      if (sub && sub.options && sub.options.applicationServerKey && toB64(sub.options.applicationServerKey) !== toB64(key)) {
        await sub.unsubscribe();
        sub = null;
      }
      sub = sub || await reg.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: key });

      await sendSubscription(sub);
      enableBtn.hidden = true;
      showHint('Push notifications are on for this device. A test notification is on its way.');
      post('/Notifications/Test');
    } finally {
      subscribing = false;
    }
  }

  function explain(err) {
    console.error('Push subscription failed', err);
    if (err && err.name === 'AbortError')
      return isEdge
        ? "Edge couldn't reach its push service. Check that Windows Settings > System > Notifications is on for Microsoft Edge, and that no VPN or firewall blocks *.notify.windows.com."
        : 'The browser push service refused the request. Check that notifications are allowed for the browser in your system settings.';
    if (err && err.name === 'NotAllowedError') return BLOCKED_HINT;
    return 'Could not enable notifications on this device: ' + (err && err.message ? err.message : 'unknown error') + '.';
  }

  // When the user allows notifications from the address bar instead of our button, finish setup automatically.
  function watchPermission(reg) {
    if (!navigator.permissions || !navigator.permissions.query) return;
    navigator.permissions.query({ name: 'notifications' }).then(status => {
      status.onchange = () => {
        if (Notification.permission === 'granted') subscribe(reg).catch(err => showHint(explain(err)));
        else if (Notification.permission === 'denied') showHint(BLOCKED_HINT);
      };
    }).catch(() => {});
  }

  async function initPush(reg) {
    if (!enableBtn) return;

    if (!pushCapable) {
      if (isIos && !isStandalone)
        showHint('To get notifications on iPhone/iPad, tap Share → Add to Home Screen, then open the app from your Home Screen.');
      return;
    }

    watchPermission(reg);
    const existing = await reg.pushManager.getSubscription();
    if (existing && Notification.permission === 'granted') {
      // Re-bind on every load so a shared device follows whoever is signed in.
      sendSubscription(existing).catch(err => console.warn('Push re-bind failed', err));
      return;
    }

    if (Notification.permission === 'denied') {
      showHint(BLOCKED_HINT);
      return;
    }

    // Permission granted but no subscription yet (e.g. allowed from the address bar): finish setup.
    if (Notification.permission === 'granted') {
      subscribe(reg).catch(err => { enableBtn.hidden = false; showHint(explain(err)); });
    } else {
      enableBtn.hidden = false;
    }

    // Must run inside the click: iOS only allows the permission prompt from a user gesture.
    enableBtn.addEventListener('click', e => {
      e.preventDefault();
      e.stopPropagation();
      subscribe(reg).catch(err => showHint(explain(err)));
    });
  }

  if ('serviceWorker' in navigator) {
    navigator.serviceWorker.addEventListener('message', e => {
      if (e.data?.type === 'app-notification') refreshFeed();
    });

    navigator.serviceWorker.register('/sw.js', { scope: '/' })
      .then(() => navigator.serviceWorker.ready)
      .then(initPush)
      .catch(err => console.error('Service worker registration failed', err));
  } else if (isIos && !isStandalone) {
    showHint('To get notifications on iPhone/iPad, tap Share → Add to Home Screen.');
  }

  // ── Install prompt (Chrome / Edge on Windows & Android) ─────────────────
  let deferredPrompt = null;
  window.addEventListener('beforeinstallprompt', e => {
    e.preventDefault();
    deferredPrompt = e;
    if (installBtn) installBtn.hidden = false;
  });
  installBtn?.addEventListener('click', async e => {
    e.preventDefault();
    if (!deferredPrompt) return;
    deferredPrompt.prompt();
    await deferredPrompt.userChoice;
    deferredPrompt = null;
    installBtn.hidden = true;
  });
  window.addEventListener('appinstalled', () => { if (installBtn) installBtn.hidden = true; });
})();
