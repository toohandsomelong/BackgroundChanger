using System.Runtime.InteropServices;

namespace BackgroundChanger
{
    class BackgroundChanger
    {
        // Used to set wallpaper
        public const int SPI_SETDESKWALLPAPER = 20;
        public const int SPIF_UPDATEINFILE = 1;
        public const int SPIF_SENDCHANGE = 2;


        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static void Main(String[] args)
        {
            Console.WriteLine("Background Changer Running!!!");
            String imgDay = "BG.jpg";
            String imgNight = "BG_Night.jpg";
            String imgNightLightOff = "BG_Night_LightOff.jpg";
            String imgDefault = "Forest Lobby.png";

            string imageName = "";
            int time = DateTime.Now.Hour;

            if (time > 0)
                imageName = imgDefault;
            if (time > 6)
                imageName = imgDay;
            if (time > 18)
                imageName = imgNight;
            if (time > 22)
                imageName = imgNightLightOff;

            Console.WriteLine("Current Hour: " + time);
            String imagePath =  Path.GetFullPath(@".bg\" + imageName);

            // Set wallpaper
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINFILE | SPIF_SENDCHANGE);
        }
    }
}