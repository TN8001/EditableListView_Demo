using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EditableListView_Demo
{
    /// <summary>EditBox内部で使う編集用Adorner</summary>
    /// <remarks>装飾なので対象サイズ等に一切影響しないのが利点</remarks>  
    internal class EditBoxAdorner : Adorner
    {
        private const int EXTRA_WIDTH = 45; // 編集に入った時の文字右側にとる余白幅
        private const int RIGHT_MARGIN = 8; // 編集エリアが最大幅になった時の右側にとる余白幅

        protected override int VisualChildrenCount => 1;

        private TextBox textBox;

        /// <summary>TextBlockにTextBoxを装飾したAdornerを初期化する</summary>
        /// <param name="adornedTextBlock">装飾対象TextBlock</param>
        /// <param name="adorningTextBox">編集用TextBox</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public EditBoxAdorner(TextBlock adornedTextBlock, TextBox adorningTextBox) : base(adornedTextBlock)
        {
            textBox = adorningTextBox ?? throw new ArgumentException("adorningTextBox is not TextBox");
            Visibility = Visibility.Hidden;
            AddVisualChild(textBox);
            AddLogicalChild(textBox);
        }

        /// <summary>表示状態を切り替える</summary>
        /// <param name="visible">true:表示</param>
        public void UpdateVisibilty(bool visible, double maxWidth)
        {
            MaxWidth = maxWidth - RIGHT_MARGIN;
            AdornedElement.Visibility = visible ? Visibility.Hidden : Visibility.Visible;
            Visibility = visible ? Visibility.Visible : Visibility.Hidden;
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
