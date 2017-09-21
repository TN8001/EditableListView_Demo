using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace EditableListView_Demo
{
    /// <summary>編集モードになるTextBlock（ListView専用 汎用ではない!!）</summary>
    /// <remarks>ListViewのGridView内で使う想定(エクスプローラの詳細モードのような動作)</remarks>  
    public class EditBox : Control
    {
        // バリデーションをつけたバインディングのキャッシュ
        private static Dictionary<ValidationRule, Binding> bindingCache = new Dictionary<ValidationRule, Binding>();
        // イベント重複登録を避けるListViewの集合
        private static HashSet<ListView> listViewSet = new HashSet<ListView>();
        // イベントを集約しているので同定用 キーボードフォーカスは一個しか持てないのでｏｋ
        private static EditBox editingControl;

        #region Text DependencyProperty 
        /// <summary>テキスト DependencyProperty</summary>
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion
        #region IsEditing DependencyProperty 
        /// <summary>編集中かどうか DependencyProperty</summary>
        public bool IsEditing { get => (bool)GetValue(IsEditingProperty); private set => SetValue(IsEditingProperty, value); }
        public static DependencyProperty IsEditingProperty
            = DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditBox),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsEditingPropertyChanged));
        private static void IsEditingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            => (sender as EditBox)?.IsEditingPropertyChanged((bool)e.NewValue);
        private void IsEditingPropertyChanged(bool value)
        {
            var left = TranslatePoint(new Point(0, 0), listView).X;
            var right = listView.ActualWidth - verticalScrollBar.ActualWidth;

            adorner.UpdateVisibilty(value, right - left);

            if(value)
            {
                editingControl = this;
                InnerText = Text;
                textBox.Focus();
            }
            else
            {
                editingControl = null;
                Text = InnerText;
                textBox.Text = InnerText;
                canBeEdit = false;
            }
        }
        #endregion

        #region InnerText internal DependencyProperty 
        // 編集中にバリデーションを掛ける用の内部Text
        internal string InnerText { get => (string)GetValue(InnerTextProperty); set => SetValue(InnerTextProperty, value); }
        internal static readonly DependencyProperty InnerTextProperty
            = DependencyProperty.Register(nameof(InnerText), typeof(string), typeof(EditBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        /// <summary>Textに掛けるバリデーションルール</summary>
        public ValidationRule ValidationRule { get; set; }

        private bool isParentSelected => listViewItem?.IsSelected ?? false;

        private EditBoxAdorner adorner;
        private TextBox textBox;
        private bool canBeEdit;
        private ListView listView;
        private ListViewItem listViewItem;
        private ScrollBar verticalScrollBar;
        private ScrollBar horizontalScrollBar;

        static EditBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditBox), new FrameworkPropertyMetadata(typeof(EditBox)));

            HorizontalAlignmentProperty.OverrideMetadata(typeof(EditBox), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
            VerticalAlignmentProperty.OverrideMetadata(typeof(EditBox), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            #region GetTemplateChild & FindAncestor
            var panel = GetTemplateChild("PART_Root") as Panel;
            if(panel == null) throw new InvalidOperationException("not find PART_Root");

            textBox = GetTemplateChild("PART_TextBox") as TextBox;
            if(textBox == null) throw new InvalidOperationException("not find PART_TextBox");

            var textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
            if(textBlock == null) throw new InvalidOperationException("not find PART_TextBlock");

            listView = this.FindAncestor<ListView>();
            if(listView == null) throw new InvalidOperationException("not find ListView");

            listViewItem = this.FindAncestor<ListViewItem>();
            if(listViewItem == null) throw new InvalidOperationException("not find ListViewItem");

            var scrollViewer = this.FindAncestor<ScrollViewer>();
            if(scrollViewer == null) throw new InvalidOperationException("not find ScrollViewer");

            verticalScrollBar = scrollViewer.Descendants<ScrollBar>().FirstOrDefault(x => x.Name == "PART_VerticalScrollBar");
            if(verticalScrollBar == null) throw new InvalidOperationException("not find ScrollViewer in PART_VerticalScrollBar");

            horizontalScrollBar = scrollViewer.Descendants<ScrollBar>().FirstOrDefault(x => x.Name == "PART_HorizontalScrollBar");
            if(horizontalScrollBar == null) throw new InvalidOperationException("not find ScrollViewer in PART_HorizontalScrollBar");
            #endregion

            // バリデーションがあった場合はキャッシュしてバインディング張り直し
            // Generic.xamlで済ませたかったがProxyやWrapperでかえってコードが増えたためやむなくこの仕様で
            if(ValidationRule != null)
            {
                if(!bindingCache.ContainsKey(ValidationRule))
                {
                    var binding = CopyBinding(textBox, TextBox.TextProperty);
                    binding.ValidationRules.Add(ValidationRule);
                    bindingCache[ValidationRule] = binding;
                }

                textBox.SetBinding(TextBox.TextProperty, bindingCache[ValidationRule]);
            }

            // TextBoxをツリーから切り離しAdornerに付け替え
            panel.Children.Remove(textBox);
            adorner = new EditBoxAdorner(textBlock, textBox);
            AdornerLayer.GetAdornerLayer(textBlock).Add(adorner);

            // 主に編集を終了させるイベント購読
            // エクスプローラでは何もないエリアのクリックや窓の移動でも編集終了するが そこまでは面倒見ない
            // オリジナルでは通常イベント購読でメモリリークがあったのでWeakEventに変更
            WeakEventManager<TextBox, KeyEventArgs>
                .AddHandler(textBox, "KeyDown", OnTextBoxKeyDown);
            WeakEventManager<TextBox, KeyboardFocusChangedEventArgs>
                .AddHandler(textBox, "LostKeyboardFocus", OnTextBoxLostKeyboardFocus);

            if(listViewSet.Add(listView)) // 既に登録されていなければ
            {
                WeakEventManager<ListView, RoutedEventArgs>
                    .AddHandler(listView, "SizeChanged", ChangeNormalMode);
                WeakEventManager<ScrollViewer, RoutedEventArgs>
                    .AddHandler(scrollViewer, "PreviewMouseWheel", ChangeNormalMode);
                WeakEventManager<ScrollBar, RoutedEventArgs>
                    .AddHandler(verticalScrollBar, "PreviewMouseDown", ChangeNormalMode);
                WeakEventManager<ScrollBar, RoutedEventArgs>
                    .AddHandler(horizontalScrollBar, "PreviewMouseDown", ChangeNormalMode);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            canBeEdit = isParentSelected ? true : false; // base呼び出しで選択される前に判定

            base.OnMouseLeftButtonDown(e);
        }
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            canBeEdit = false;
        }
        // シングルクリックとダブルクリックの切り分けは正確にはタイマー等で待って判定するが
        // 面倒なのでやらない 一瞬編集モードに入るがまあ気にしない
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if(IsEditing) return;

            if(canBeEdit) IsEditing = true;
        }

        private static void ChangeNormalMode(object sender, RoutedEventArgs e)
        {
            if(editingControl == null) return;

            editingControl.IsEditing = false;
        }

        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if(!IsEditing) return;

            if(e.Key == Key.Enter || e.Key == Key.F3) // 編集終了
                IsEditing = false;

            if(e.Key == Key.Escape) // 編集キャンセル
            {
                InnerText = Text;
                IsEditing = false;
            }
        }
        private void OnTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(e.NewFocus is ContextMenu) return;

            IsEditing = false;
        }

        // 必要なところだけ 雑い
        private Binding CopyBinding(DependencyObject obj, DependencyProperty prop)
        {
            var binding = BindingOperations.GetBinding(obj, prop);
            return new Binding()
            {
                Path = binding.Path,
                RelativeSource = binding.RelativeSource,
                Mode = binding.Mode,
                UpdateSourceTrigger = binding.UpdateSourceTrigger
            };
        }
    }
}
