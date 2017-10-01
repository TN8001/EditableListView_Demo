using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EditableListView_Demo
{
    /// <summary>EditBlock内部で使う編集用Adorner</summary>
    /// <remarks>装飾なので対象サイズ等に一切影響しないのが利点</remarks>  
    internal class EditBlockAdorner : Adorner
    {
        private const int EXTRA_WIDTH = 45; // 編集に入った時の文字右側にとる余白幅
        private const int RIGHT_MARGIN = 0; // 編集エリアが最大幅になった時の右側にとる余白幅

        protected override int VisualChildrenCount => 1;

        private TextBox textBox;

        /// <summary>TextBlockにTextBoxを装飾したAdornerを初期化する</summary>
        /// <param name="adornedTextBlock">装飾対象TextBlock</param>
        /// <param name="adorningTextBox">編集用TextBox</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public EditBlockAdorner(TextBlock adornedTextBlock, TextBox adorningTextBox) : base(adornedTextBlock)
        {
            textBox = adorningTextBox ?? throw new ArgumentException("adorningTextBox is not TextBox");
            Visibility = Visibility.Hidden;
            AddVisualChild(textBox);
            AddLogicalChild(textBox);
        }

        /// <summary>表示状態を切り替える</summary>
        /// <param name="visible">true:表示</param>
        /// <param name="maxWidth">最大幅</param>
        public async Task UpdateVisibilty(bool visible, double maxWidth)
        {
            MaxWidth = maxWidth - RIGHT_MARGIN;
            AdornedElement.Visibility = visible ? Visibility.Hidden : Visibility.Visible;
            Visibility = visible ? Visibility.Visible : Visibility.Hidden;

            if(visible) // ScrollViewer等があった場合フォーカスが取られてしまうので仕方なく遅延処理
            {
                await Task.Delay(100);
                textBox.Focus();
            }
        }

        protected override Visual GetVisualChild(int index) => textBox;
        protected override Size MeasureOverride(Size constraint)
        {
            textBox.Measure(constraint);
            var width = textBox.DesiredSize.Width + EXTRA_WIDTH;
            width = width > MaxWidth ? MaxWidth : width;
            var height = textBox.DesiredSize.Height;
            return new Size(width, height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            textBox.Arrange(new Rect(finalSize));
            return finalSize;
        }
    }
}
