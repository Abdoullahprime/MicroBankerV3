// src/BaseTemplate.Web/Models/NavLinkModel.cs
namespace BaseTemplate.Web.Models;

/// <summary>One sidebar entry. Module = permission module required to see it (null = always visible).</summary>
public record NavLinkModel(string? Module, string Controller, string Action, string Icon, string Text, string Page);
