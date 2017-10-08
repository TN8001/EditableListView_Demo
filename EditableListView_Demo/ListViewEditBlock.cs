using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace EditableListView_Demo
{
    /// <summary>選択後クリックで編集モードになるTextBlock（ListBox&ListView専用）</summary>
    /// <remarks>エクスプローラの詳細モードのような動作</remarks>  
    public class ListEditBlock : EditBlock
    {
        // イベント重複登録を避けるListBoxの集合
        private static HashSet<ListBox> listBoxSet = new HashSet<ListBox>();
        // シングルクリックダブルクリック判別用タイマ
        private static DispatcherTimer timer = new DispatcherTimer();
        private static ListEditBlock clickedEditBlock;

        private bool isParentSelected => llistBoxItem?.IsSelected ?? false;

        private ListBoxItem llistBoxItem;
        private bool canBeEdit;

        static ListEditBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListEditBlock), new FrameworkPropertyMetadata(typeof(ListEditBlock)));

            timer.Tick += Timer_Tick;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var listBox = this.FindAncestor<ListBox>();
            if(listBox == null) throw new InvalidOperationException("not find ListBox or ListView");

            var scrollViewer = this.FindAncestor<ScrollViewer>();
            if(scrollViewer == null) throw new InvalidOperationException("not find ScrollViewer");

            llistBoxItem = this.FindAncestor<ListBoxItem>();
            if(llistBoxItem == null) throw new InvalidOperationException("not find ListBoxItem or ListViewItem");

            ScrollBar scrollBar1;
            ScrollBar scrollBar2;
            if(listBox is ListView)
            {
                scrollBar1 = scrollViewer.Descendants<ScrollBar>().FirstOrDefault(x => x.Name == "PART_VerticalScrollBar");
                if(scrollBar1 == null) throw new InvalidOperationException("not find ScrollViewer in PART_VerticalScrollBar");

                scrollBar2 = scrollViewer.Descendants<ScrollBar>().FirstOrDefault(x => x.Name == "PART_HorizontalScrollBar");
                if(scrollBar2 == null) throw new InvalidOperationException("not find ScrollViewer in PART_HorizontalScrollBar");
            }
            else // ListBox
            {
                scrollBar1 = scrollViewer.Descendants<ScrollBar>().ElementAtOrDefault(0);
                if(scrollBar1 == null) throw new InvalidOperationException("not find ScrollViewer in VerticalScrollBar");

                scrollBar2 = scrollViewer.Descendants<ScrollBar>().ElementAtOrDefault(1);
                if(scrollBar2 == null) throw new InvalidOperationException("not find ScrollViewer in HorizontalScrollBar");
            }

            // 既に登録されていればスキップ
            if(!listBoxSet.Add(listBox)) return;

            // 編集を終了させるイベント購読
            // エクスプローラでは何もないエリアのクリックや窓の移動でも編集終了するが
            // そこまでは面倒見ない（使用側で対応したほうがすっきりすると思うので）
            WeakEventManager<ScrollViewer, RoutedEventArgs>
                .AddHandler(scrollViewer, "PreviewMouseWheel", EndEdit);
            WeakEventManager<ScrollBar, RoutedEventArgs>
                .AddHandler(scrollBar1, "PreviewMouseDown", EndEdit);
            WeakEventManager<ScrollBar, RoutedEventArgs>
                .AddHandler(scrollBar2, "PreviewMouseDown", EndEdit);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            canBeEdit = isParentSelected; // base呼び出しで選択される前に判定
            clickedEditBlock = this;

            base.OnMouseLeftButtonDown(e);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if(!canBeEdit) return;
            if(clickedEditBlock != this) return;

            timer.Interval = TimeSpan.FromMilliseconds(GetDoubleClickTime());
            timer.Start();
        }
        // baseクラスの動作抑制 
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            canBeEdit = false;
            e.Handled = true;
        }

        // ダブルクリック判定時間まで待機して シングルクリックが確定したら編集開始
        // 編集開始までにワンテンポラグがある（エクスプローラと同じ）
        private static void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            if(!clickedEditBlock.canBeEdit) return;

            clickedEditBlock.IsEditing = true;
            clickedEditBlock = null;
        }
        // イケてないと思うがどうすりゃいいのか。。。
        private static void EndEdit(object sender, RoutedEventArgs e)
        {
            if(NowEditing == null) return;

            var s = ((DependencyObject)sender).FindAncestor<ListBox>();
            var n = NowEditing.FindAncestor<ListBox>();

            if(s == n) EndEdit();
        }

        // ダブルクリック判定時間をミリセカンドで返す
        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();
    }
}
