namespace Icarus.UI
{
    public class BooleanToStringConverter : BooleanConverter<string>
    {
        public BooleanToStringConverter() :
            base("True", "False")
        { }
    }
}
