using BitPantry.Parsing.Strings;
using System.Text.Json;

namespace BitPantry.Tabs.Web
{
    public abstract class SessionStateBase
    {
        private readonly string _keyPrefix;
        private IHttpContextAccessor _httpCtx;

        protected SessionStateBase(IHttpContextAccessor httpCtx)
        {
            _httpCtx = httpCtx;
        }

        protected ISession Session => _httpCtx.HttpContext.Session;

        protected SessionStateBase(
            IHttpContextAccessor httpCtx,
            string keyPrefix)
        {
            _httpCtx = httpCtx;
            _keyPrefix = keyPrefix;
        }

        protected string GetValue(string key)
        {
            try
            {
                return GetValue<string>(key);
            }
            catch (Exception ex)
            {
                throw CreateSessionStateException(key, ex);
            }
        }

        protected void SetValue(string key, string value)
            => SetValue<string>(key, value);

        protected TType GetValue<TType>(string key)
        {
            try
            {
                if(StringParsing.GetParser<TType>() != null)
                    return StringParsing.Parse<TType>(Session.GetString(FormatKey(key)));
                else
                    return Deserialize<TType>(Session.Get(FormatKey(key)));
            }
            catch (Exception ex)
            {
                throw CreateSessionStateException(key, ex);
            }
        }

        protected void SetValue<TType>(string key, TType value) 
        {
            try
            {
                if (StringParsing.GetParser<TType>() != null)
                    Session.SetString(FormatKey(key), value.ToString());
                else
                    Session.Set(FormatKey(key), Serialize(value));
            }
            catch (Exception ex)
            {
                throw CreateSessionStateException(key, ex);
            }
        }   

        protected TType GetValue<TType>(string key, TType defaultValue) 
        {
            try
            {
                if (StringParsing.GetParser<TType>() != null)
                {
                    return StringParsing.SafeParse(Session.GetString(FormatKey(key)), defaultValue);
                }
                else
                {
                    var bin = Session.Get(FormatKey(key));
                    if(bin == null)
                        return defaultValue;
                    else
                        return Deserialize<TType>(bin);
                }
            }
            catch (Exception ex)
            {
                throw CreateSessionStateException(key, ex);
            }
        }

        private string FormatKey(string key) { return string.IsNullOrEmpty(_keyPrefix) ? key : $"{_keyPrefix}:{key}"; }

        private SessionStateException CreateSessionStateException(string key, Exception innerException)
        {
            return new SessionStateException(
                $"An application setting error occurred while getting the value for key \"{FormatKey(key)}\"",
                innerException);
        }

        private byte[] Serialize(object obj)
            => JsonSerializer.SerializeToUtf8Bytes(obj);

        private TType Deserialize<TType>(byte[] data)
            => JsonSerializer.Deserialize<TType>(data);
    }
}

