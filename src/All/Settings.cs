using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu.Remix.MixedUI;
using NCRMarauder.MarauderCat;
using static Menu.Remix.MixedUI.OpTextBox;

namespace ncrcatoptions
{
    public class Options : OptionInterface
    {
        public static Configurable<bool> MarMaulsPups { get; set; }

        public Options()
        {
            ConfigHolder config = this.config;
            string key = "MarMaulsPups";
            MarMaulsPups = config.Bind<bool>(key, true, new ConfigurableInfo(
                "If Marauder should be able to maul pups and other cats, regardless of friendly fire.", null, "Marauder Mauling", Array.Empty<object>()));
        }

        public override void Initialize()
        {
            OpTab opTab = new OpTab(this, OptionInterface.Translate("Options"));
            this.Tabs = new OpTab[]
            {
                opTab
            };
            OpContainer tab1Container = new OpContainer(new Vector2(0f, 0f));
            opTab.AddItems(new UIelement[]
            {
                tab1Container
            });
            UIelement[] UIArrayElements = new UIelement[]
            {
                new OpCheckBox(MarMaulsPups, new Vector2(10f, 500f))
                {
                    description = OptionInterface.Translate("Marauder Mauling")
                },
                new OpLabel(65f, 540f, OptionInterface.Translate("If Marauder should be able to maul pups and other cats, regardless of friendly fire."), false)
            };
            opTab.AddItems(UIArrayElements);
        }
    }
}
