// Service worker: static caching, offline fallback and Web Push display.
const CACHE_VERSION = 'app-static-v1';
const OFFLINE_URL   = '/offline.html';
const ICON          = '/assets/img/favicons/android-chrome-192x192.png';
const PRECACHE      = [OFFLINE_URL, ICON];
// App scripts (/js/) are not cached so fixes reach users on the next load; only theme assets are.
const STATIC_PREFIXES = ['/assets/', '/vendors/', '/lib/', '/css/'];

self.addEventListener('install', event => {
  event.waitUntil(caches.open(CACHE_VERSION).then(c => c.addAll(PRECACHE)).then(() => self.skipWaiting()));
});

self.addEventListener('activate', event => {
  event.waitUntil(
    caches.keys()
      .then(keys => Promise.all(keys.filter(k => k !== CACHE_VERSION).map(k => caches.delete(k))))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', event => {
  const req = event.request;
  if (req.method !== 'GET') return;
  const url = new URL(req.url);
  if (url.origin !== self.location.origin) return;

  // Pages hold personal data: always go to the network, never cache them.
  if (req.mode === 'navigate') {
    event.respondWith(fetch(req).catch(() => caches.match(OFFLINE_URL)));
    return;
  }

  // Theme assets: stale-while-revalidate.
  if (STATIC_PREFIXES.some(p => url.pathname.startsWith(p))) {
    event.respondWith(
      caches.open(CACHE_VERSION).then(async cache => {
        const cached  = await cache.match(req);
        const network = fetch(req).then(res => {
          if (res.ok) cache.put(req, res.clone());
          return res;
        }).catch(() => cached);
        return cached || network;
      })
    );
  }
});

self.addEventListener('push', event => {
  let data = {};
  try { data = event.data ? event.data.json() : {}; }
  catch { data = { body: event.data ? event.data.text() : '' }; }

  const title = data.title || 'Base Template';
  const options = {
    body: data.body || '',
    icon: ICON,
    tag:  data.tag || undefined,
    renotify: !!data.tag,
    data: { url: data.url || '/', id: data.id || null }
  };

  // iOS revokes push permission if a push arrives without a visible notification, so always show one.
  event.waitUntil(Promise.all([
    self.registration.showNotification(title, options),
    self.clients.matchAll({ type: 'window', includeUncontrolled: true })
      .then(clients => clients.forEach(c => c.postMessage({ type: 'app-notification' })))
  ]));
});

self.addEventListener('notificationclick', event => {
  event.notification.close();
  const { url, id } = event.notification.data || {};
  const hasId  = id && id !== '00000000-0000-0000-0000-000000000000';
  const target = new URL(hasId ? `/Notifications/Open/${id}` : (url || '/'), self.location.origin).href;

  event.waitUntil(
    self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then(clients => {
      for (const client of clients) {
        if (new URL(client.url).origin === self.location.origin && 'focus' in client) {
          return client.focus()
            .then(() => client.navigate(target))
            .catch(() => self.clients.openWindow(target));
        }
      }
      return self.clients.openWindow(target);
    })
  );
});
