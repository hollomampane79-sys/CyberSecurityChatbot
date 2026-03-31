using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace CyberSecurityChatbot.Utils
{
    public static class AudioPlayer
    {
        public static void PlayGreeting()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "greeting.wav");
                SoundPlayer player = new SoundPlayer(path);
                player.PlaySync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Audio could not be played.");
            }
        }
    }
}
