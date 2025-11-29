using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;

namespace TrucoClient.Helpers.UI
{
    public static class CursorManager
    {
        private static readonly Cursor clickCursor;
        // private static readonly Cursor loadingCursor;
        // private static readonly Cursor handCursor;

        static CursorManager()
        {
            string basePath = "pack://application:,,,/TrucoClient;component/Resources/Cursors/";

            clickCursor = LoadCustomCursor(basePath + "cursorClick.cur");
            // loadingCursor = LoadCustomCursor(basePath + "cursorLoading.ani");
            // handCursor = LoadCustomCursor(basePath + "cursorHand.cur");
        }

        private static Cursor LoadCustomCursor(string resourcePath)
        {
            try
            {
                Uri uri = new Uri(resourcePath, UriKind.Absolute);
                StreamResourceInfo info = Application.GetResourceStream(uri);

                if (info == null)
                {
                    throw new FileNotFoundException("Resource not found.");
                }

                return new Cursor(info.Stream);
            }
            catch (Exception)
            {
                return Cursors.Arrow;
            }
        }

        public static Cursor Click()
        {
            return clickCursor;
        }

        /*
        public static Cursor Loading()
        {
            return loadingCursor;
        }

        public static Cursor Hand()
        {
            return handCursor;
        }
        */
    }
}
