using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCDebugTool
{
    public static class Helper
    {
        public static TryHelper Try(Action a)
        {
            TryHelper t = new TryHelper();
            t.Try(a);
            return t;
        }
    }

    public class TryHelper
    {
        public Action BodyTry;
        public Action<Exception> BodyCatch;
        public Action BodyFinally;
        public void Do()
        {
            try
            {
                BodyTry.Invoke();
            }
            catch (Exception ex)
            {
                BodyCatch?.Invoke(ex);
            }
            finally
            {
                BodyFinally?.Invoke();
            }
        }
        public TryHelper Try(Action a)
        {
            this.BodyTry = a;
            return this;
        }

        public TryHelper Catch(Action<Exception> a)
        {
            this.BodyCatch = a;
            return this;
        }
        public TryHelper Finally(Action a = null)
        {
            this.BodyFinally = a;
            if (BodyTry != null && BodyCatch != null)
            {
                Do();
            }
            return this;

        }
    }
}
