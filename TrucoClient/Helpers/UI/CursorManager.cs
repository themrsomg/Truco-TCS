using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;

namespace TrucoClient.Helpers.UI
{
    public static class CursorManager
    {
        private static readonly string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string basePath = $"pack://application:,,,/{assemblyName};component/Resources/Cursors/";

        public static readonly Cursor clickCursor = LoadCustomCursor(basePath + "cursorClick.cur");

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
            catch (FileNotFoundException)
            {
                return Cursors.Arrow;
            }
            catch (UriFormatException)
            {
                return Cursors.Arrow;
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
    }
}
