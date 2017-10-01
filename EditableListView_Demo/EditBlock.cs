using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        // 編集に入っているEditBlockの同定用
        protected static EditBlock NowEditing { get; private set; }

        // バリデーションをつけたバインディングのキャッシュ
        private static Dictionary<ValidationRule, Binding> bindingCache = new Dictionary<ValidationRule, Binding>();
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

            // 最大幅設定
            var adornerLayer = adorner.Parent as AdornerLayer;
            var left = TranslatePoint(new Point(0, 0), adornerLayer).X;
            var right = adornerLayer.ActualWidth;
            // #pragma warning disable CS4014  抑制のための_ 意味はない
            var _ = adorner.UpdateVisibilty(value, right - left);

            if(value)
                InnerText = Text;
            else
            {
                Text = InnerText;
                textBox.Text = InnerText;
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

        private EditBlockAdorner adorner;
        private TextBox textBox;

        static EditBlock()
        {
            // クラスハンドラー化 WeakEventより効率ＵＰ？？
            EventManager.RegisterClassHandler(typeof(TextBox), PreviewKeyDownEvent, new KeyEventHandler(Class_OnPreviewKeyDown));
            EventManager.RegisterClassHandler(typeof(TextBox), LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(Class_OnLostKeyboardFocus));

            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditBlock), new FrameworkPropertyMetadata(typeof(EditBlock)));
        }

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

        public override void OnApplyTemplate()
        {
            if(DesignerProperties.GetIsInDesignMode(this)) return;
            base.OnApplyTemplate();

            var panel = GetTemplateChild("PART_Root") as Panel;
            if(panel == null) throw new InvalidOperationException("not find PART_Root");

            textBox = GetTemplateChild("PART_TextBox") as TextBox;
            if(textBox == null) throw new InvalidOperationException("not find PART_TextBox");

            var textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
            if(textBlock == null) throw new InvalidOperationException("not find PART_TextBlock");

            SetValidation();

            // TextBoxをツリーから切り離しAdornerに付け替え
            panel.Children.Remove(textBox);
            adorner = new EditBlockAdorner(textBlock, textBox);
            var adornerLayer = AdornerLayer.GetAdornerLayer(textBlock);
            adornerLayer.Add(adorner);

            // 既にイベント登録されていればスキップ
            if(!adornerLayerSet.Add(adornerLayer)) return;

            // イベント登録先はやや無駄があるが多様なパターンに対応できそうなAdornerLayerにしてみた
            WeakEventManager<AdornerLayer, SizeChangedEventArgs>
                .AddHandler(adornerLayer, "SizeChanged", (s, e) => EndEdit());
        }

        // バリデーションがあった場合はキャッシュしてバインディング張り直し
        // Generic.xamlで済ませたかったがProxyやWrapperでかえってコードが増えたためやむなくこの仕様で
        private void SetValidation()
        {
            if(ValidationRule == null) return;

            if(!bindingCache.ContainsKey(ValidationRule))
            {
                var binding = CopyBinding(textBox, TextBox.TextProperty);
                binding.ValidationRules.Add(ValidationRule);
                bindingCache[ValidationRule] = binding;
            }

            textBox.SetBinding(TextBox.TextProperty, bindingCache[ValidationRule]);
        }

        // 編集開始
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            IsEditing = true;
        }

        // 必要なところだけ 雑い
        private static Binding CopyBinding(DependencyObject obj, DependencyProperty prop)
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
