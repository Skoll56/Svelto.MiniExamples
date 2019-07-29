using ServiceLayer;
using Svelto.ECS.Unity;
using TMPro;
using UnityEngine;

namespace Boxtopia.GUIs.LocalisedText
{
    public class LabelTextProImplementor : MonoBehaviour, ILabelText, IImplementor
    {
        public string LocalisationKey;

        public GameStringsID textKey
        {
            get
            {
                if (_key == GameStringsID.NOT_INITIALIZED)
                    _key = LocalizationService.VerySlowParseEnum(LocalisationKey);
                
                return _key;
            }
        }

        public string text
        {
            set
            {
                if (_reference == null)
                {
                    _reference = GetComponent<TextMeshProUGUI>();
                }

                _reference.text = value;
            }
        }

        TextMeshProUGUI                 _reference;
        GameStringsID _key;
    }
}