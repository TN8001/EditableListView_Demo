using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace EditableListView_Demo
{

    /// <summary>ダブルクリックで編集モードになるTextBlock（汎用）</summary>
    /// <remarks>ListBox ListViewに最適化していないのでスクロール等で編集終了しない</remarks>  
    public class EditBlock : Control
    {
        /// <summary>現在編集モードに入っているEditBlock</summary>
        protected static EditBlock NowEditing { get; private set; }

        // イベント登録の重複を避ける集合
        private static HashSet<AdornerLayer> adornerLayerSet = new HashSet<AdornerLayer>();

        #region Text DependencyProperty 
        /// <summary>テキストを設定取得する DependencyProperty</summary>
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditBlock),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion
        #region IsEditing DependencyProperty 
        /// <summary>編集モードかどうかを設定取得する DependencyProperty</summary>
        public bool IsEditing { get => (bool)GetValue(IsEditingProperty); set => SetValue(IsEditingProperty, value); }
        public static DependencyProperty IsEditingProperty
            = DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditBlock),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsEditingPropertyChanged));
        private static void IsEditingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            => (sender as EditBlock)?.IsEditingPropertyChanged((bool)e.NewValue);
        private void IsEditingPropertyChanged(bool value)
        {
            if(value && NowEditing != this) EndEdit();

            var textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
            var adornerLayer = AdornerLayer.GetAdornerLayer(textBlock);
            if(adornerLayerSet.Add(adornerLayer))
                WeakEventManager<AdornerLayer, SizeChangedEventArgs>
                    .AddHandler(adornerLayer, "SizeChanged", (s, e) => EndEdit());

            if(value)
            {
                var textBox = new TextBox() { Margin = new Thickness(-3, -1, 0, 0) };
                textBox.SetBinding(TextBox.TextProperty, CreateBinding());

                adorner = new EditBlockAdorner(textBlock, textBox);
                adornerLayer.Add(adorner);

                // 最大幅設定
                var left = TranslatePoint(new Point(0, 0), adornerLayer).X;
                var right = adornerLayer.ActualWidth;
                // #pragma warning disable CS4014  抑制のための_ 意味はない
                var _ = adorner.UpdateVisibilty(value, right - left);

                InnerText = Text;
            }
            else
            {
                Text = InnerText;
                var _ = adorner.UpdateVisibilty(value, 0);
                adornerLayer.Remove(adorner);
                adorner = null;
            }

            NowEditing = value ? this : null;
        }
        #endregion

        #region InnerText internal DependencyProperty 
        // 編集中にバリデーションを掛ける用の内部Text
        internal string InnerText { get => (string)GetValue(InnerTextProperty); set => SetValue(InnerTextProperty, value); }
        internal static readonly DependencyProperty InnerTextProperty
            = DependencyProperty.Register(nameof(InnerText), typeof(string), typeof(EditBlock),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        /// <summary>Textに掛けるバリデーションルール</summary>
        public ValidationRule ValidationRule { get; set; }

        // 編集中以外はnull
        private EditBlockAdorner adorner;

        static EditBlock()
        {
            // クラスハンドラー化 WeakEventより効率ＵＰ？？
            EventManager.RegisterClassHandler(typeof(TextBox), PreviewKeyDownEvent, new KeyEventHandler(Class_OnPreviewKeyDown));
            EventManager.RegisterClassHandler(typeof(TextBox), LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(Class_OnLostKeyboardFocus));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditBlock), new FrameworkPropertyMetadata(typeof(EditBlock)));
        }

        /// <summary>現在編集モードに入っているEditBlockの編集を終了</summary>
        public static void EndEdit()
        {
            if(NowEditing != null) NowEditing.IsEditing = false;
        }

        private static void Class_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(NowEditing == null) return;
            if(e.NewFocus is ContextMenu) return;

            EndEdit();
        }
        private static void Class_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(NowEditing == null) return;

            if(e.Key == Key.Enter || e.Key == Key.F3) // 編集終了
                EndEdit();

            if(e.Key == Key.Escape) // 編集キャンセル
            {
                NowEditing.InnerText = NowEditing.Text;
                EndEdit();
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            IsEditing = true;
        }

        private Binding CreateBinding()
        {
            var binding = new Binding()
            {
                Path = new PropertyPath("InnerText"),
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };

            if(ValidationRule != null)
                binding.ValidationRules.Add(ValidationRule);

            return binding;
        }
    }
}
