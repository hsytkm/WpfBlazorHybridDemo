using System.Diagnostics;
using System.Windows;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;

namespace WpfBlazorHybridDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
        Resources.Add("services", serviceCollection.BuildServiceProvider());
    }

    private void BlazorWebViewInitialized(object sender, BlazorWebViewInitializedEventArgs e)
    {
        // [Blazor Hybrid で F5 キーでリフレッシュなどのブラウザ固有のキー操作を無効化したい](https://zenn.dev/microsoft/articles/blazor-hybrid-disable-fkeys)
        // F5 キーなどを無効化する
        e.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;

        // [Blazor Hybrid でテキストボックスの右クリックメニューを表示する](https://zenn.dev/microsoft/articles/blazor-hybrid-editable-context-menu)
        // コンテキストメニューを有効化
        e.WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
        e.WebView.CoreWebView2.ContextMenuRequested += CoreWebView2_ContextMenuRequested;
    }

    private void CoreWebView2_ContextMenuRequested(object? sender, CoreWebView2ContextMenuRequestedEventArgs e)
    {
        //Debug.WriteLine($"IsEditable? {e.ContextMenuTarget.IsEditable}");

        // 編集可能な項目の場合には、不要なメニューを削除する
        if (e.ContextMenuTarget.IsEditable)
        {
            // For editable elements such as <input> and <textarea> we enable the context menu but remove items we don't want in this app
            string[] itemNamesToRemove = ["share", "webSelect", "webCapture", "inspectElement"];
            var menuIndexesToRemove = e.MenuItems
                .Select((menu, index) => (menu, index))
                .Where(x => itemNamesToRemove.Contains(x.menu.Name))
                .Select(x => x.index)
                .Reverse()
                .ToArray();

            Debug.WriteLine($"Removing these indexes: {string.Join(", ", menuIndexesToRemove.Select(i => i.ToString()))}");
            foreach (int menuIndexToRemove in menuIndexesToRemove)
            {
                Debug.WriteLine($"Removing {e.MenuItems[menuIndexToRemove].Name}...");
                e.MenuItems.RemoveAt(menuIndexToRemove);
            }

            // Trim extra separators from the end
            while (e.MenuItems[^1].Kind is CoreWebView2ContextMenuItemKind.Separator)
            {
                e.MenuItems.RemoveAt(e.MenuItems.Count - 1);
            }
        }
        else
        {
            // For non-editable elements such as <div> we disable the context menu
            e.Handled = true;
        }
    }

}