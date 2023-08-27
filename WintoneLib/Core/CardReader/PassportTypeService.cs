using System.Collections.Specialized;

namespace WintoneLib.Core.CardReader
{
    public class PassportTypeService
    {
        private NameValueCollection PassportTypeList;

        public PassportTypeService()
        {
            PassportTypeList = new NameValueCollection()
                {
                    {"2","居民身份证-照片页" },
                    {"3","居民身份证-签发机关页" },
                    {"4","临时居民身份证" },
                    {"5","机动车驾驶证" },
                    {"6","机动车行驶证" },
                    {"14","港澳居民来往内地通行证-照片页" },
                    {"15","港澳居民来往内地通行证-机读码页" },
                    {"1005","澳门居民身份证-照片页" },
                };
        }

        public string GetName(string key)
        {
            var result = PassportTypeList[key];

            if (string.IsNullOrEmpty(result))
                result = "无法识别证件类型";

            return result;
        }
    }
}
