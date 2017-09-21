using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace EditableListView_Demo
{
    // 雑いので注意
    ///<summary>ファイル名に使えるか検証 <seealso cref="Path.GetInvalidFileNameChars"/>を使用</summary>
    public class FileNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value == null) return new ValidationResult(false, "value is Null");

            if(value is string s)
            {
                if(s == "") return new ValidationResult(false, "value is Empty");

                if(s.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    return new ValidationResult(false, "Invalid value");

                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "value is not string");
        }
    }

    // 複数のバリデーションテスト 特に意味はない
    ///<summary>*（半角アスタリスク）だけか検証</summary>
    public class AsteriskValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value == null) return new ValidationResult(false, "value is Null");

            if(value is string s)
            {
                if(s.Replace("*", "").Length > 0)
                    return new ValidationResult(false, "Invalid value");

                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "value is not string");
        }
    }
}
