using MioGame;
using MioGame.std;
using MioModdingApi;
using MioModLoader;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenWorldVessel
{
    public class OpenWorldVessel : Mod
    {

        public override void Initialize()
        {
            Hooks();

            ConnectMenus();
        }

        public static List<string> GetData()
        {
            List<string> result = [];
            using (StreamReader file = System.IO.File.OpenText(@".\mods\OpenWorldVessel\shortcuts.dat"))
            {
                string asText = file.ReadToEnd();
                foreach (string line in asText.Split("\n"))
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    result.Add(line);
                }
            }
            return result;
        }


        private static ModMenuPage? modPage;
        private static ButtonOption? activate;
        public void ConnectMenus()
        {
            modPage = ModMenu.RegisterMod(this, "OpenWorldVessel", "");
            activate = modPage.RegisterButton("activate", "Activate", Hooks);
        }

        private unsafe void Hooks()
        {
            On.MioGame.On_Game.fixed_update.Prefix += Fixed_update_Prefix;
        }

        private int startDelay = 20;      

        private unsafe void Fixed_update_Prefix(Game* self)
        {
            if (startDelay > 1)
            {
                startDelay--;
            }
            ref var game = ref Unsafe.AsRef<Game>(self);
            ref var save = ref self->save;
            List<MioGame.String> strings = [];
            foreach (string entry in GetData())
            {
                strings.Add(Util.StringToMioString(entry));
            }

            if (startDelay >= 1)
            {
                fixed (MioGame.String* ptr = CollectionsMarshal.AsSpan(strings))
                {
                    for (int i = 0; i < strings.Count; i++)
                    {
                        try
                        {
                            save.acquire(ptr + i);
                        }
                        catch (Exception ex)
                        {
                            LogMessage("[OWV] Could not give shortcut: " + ex);
                        }
                    }
                }
                startDelay--;
            }
            else
            {
                fixed (MioGame.String* ptr = CollectionsMarshal.AsSpan(strings))
                {
                    for (int i = 0; i < strings.Count; i++)
                    {
                        ref Save_entry entry = ref Unsafe.AsRef<Save_entry>(save.peek(ptr + i));
                        entry.count = 1;
                    }
                }
                On.MioGame.On_Game.fixed_update.Prefix -= Fixed_update_Prefix;
            }   
        }
    }
}
