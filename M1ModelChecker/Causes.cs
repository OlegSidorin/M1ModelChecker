namespace M1ModelChecker
{
    public enum Causes
    {
        UnknownState = 0,
        WrongGuidAndName = 1,
        GoodNameWrongGuid = 2,
        GoodGuidWrongName = 3,
        OK = 4
    }
    public static class ErrorLevelExtensions
    {
        public static string ToFriendlyString(this Causes cause)
        {
            switch (cause)
            {
                case Causes.OK:
                    return "с параметром все ок";
                case Causes.WrongGuidAndName:
                    return "параметр не из ФОП";
                case Causes.GoodGuidWrongName:
                    return "guid ок, имя нет";
                case Causes.GoodNameWrongGuid:
                    return "имя ок, guid не верен";
                case Causes.UnknownState:
                    return "не понятно";
                default:
                    return "?";
            }
        }
    }
}