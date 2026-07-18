#nullable enable
using System;

namespace ReactiveUITK.Samples.UIs.PrettyUi.UI.Components.Sidebar {
  public class SidebarItem {
    public string Id { get; set; } = null!;
    public string Label { get; set; } = null!;
    public Action? OnClick { get; set; }
    public bool IsActive { get; set; }
    public bool Disabled { get; set; }
  }
}
