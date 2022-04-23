namespace VTS.Models.Impl{
    public class JsonUtilityImpl : IJsonUtility
    {
        public T FromJson<T>(string json)
        {
            return UnityEngine.JsonUtility.FromJson<T>(json);
        }

        public string ToJson(object obj)
        {
            string json = UnityEngine.JsonUtility.ToJson(obj);
            return RemoveNullProps(json);
        }

        private string RemoveNullProps(string input){
            string[] props = input.Split(',', '{', '}');
            string output = input;
            foreach(string prop in props){
                // We're doing direct string manipulation on a serialized JSON, which is incredibly frail.
                // Please forgive my sins, as Unity's builtin JSON tool uses default field values instead of nulls,
                // and sometimes that is unacceptable behavior.
                // I'd use a more robust JSON library if I wasn't publishing this as a plugin.
                string[] pair = prop.Split(':');
                if(pair.Length > 1){
                    float nullable = 0.0f;
                    float.TryParse(pair[1], out nullable);
                    if(float.MinValue.Equals(nullable)){
                        output = output.Replace(prop+",", "");
                        output = output.Replace(prop, "");
                    }
                    else if("\"\"".Equals(pair[1])){
                        output = output.Replace(prop+",", "");
                        output = output.Replace(prop, "");
                    }
                }
            }
            output = output.Replace(",}", "}");
            return output;
        }
    }
}
