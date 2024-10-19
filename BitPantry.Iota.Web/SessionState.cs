namespace BitPantry.Iota.Web
{
    public class SessionState : SessionStateBase
    {
        public SessionState(IHttpContextAccessor httpCtx) : base(httpCtx) { }

        public ReviewProgress ReviewProgress 
        {
            get => GetValue<ReviewProgress>(nameof(ReviewProgress));
            set => SetValue(nameof(ReviewProgress), value);
        }
    }

}
