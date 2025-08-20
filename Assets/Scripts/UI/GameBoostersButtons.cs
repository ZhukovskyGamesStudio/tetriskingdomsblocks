using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameBoostersButtons : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI _randomFieldCountText;

    [SerializeField]
    private TextMeshProUGUI _dinamyteCountText;

    [SerializeField]
    private TextMeshProUGUI _hummerCountText;

    [SerializeField]
    private TextMeshProUGUI _rotatePieceCountText;

    [SerializeField]
    private Button _randomFieldButton;
    

    [SerializeField]
    public Button _hummerButton,_dinamyteButton, ShuffleButton;

    [SerializeField]
    private Button _rotatePieceButton;

    [SerializeField]
    private Image _dynamiteImageButton;

    [SerializeField]
    private Image _hummerImageButton;

    [SerializeField]
    private Image _randomImageButton;

    [SerializeField]
    private Image _rotateImageButton;

    public void SetBoosterButtons(BoostersConfig config, int curLevel) {
        var lockSprite = config.LockBoosterSprite;

        if (config.RandomUnlockLevel > curLevel+1) {
            _randomImageButton.sprite = lockSprite;
            _randomFieldButton.enabled = false;
            _randomFieldCountText.text = (config.RandomUnlockLevel).ToString();
        }

        if (config.DynamiteUnlockLevel > curLevel+1) {
            _dynamiteImageButton.sprite = lockSprite;
            _dinamyteButton.enabled = false;
            _dinamyteCountText.text = (config.DynamiteUnlockLevel).ToString();
        }

        if (config.HammerUnlockLevel > curLevel+1) {
            _hummerImageButton.sprite = lockSprite;
            _hummerButton.enabled = false;
            _hummerCountText.text = (config.HammerUnlockLevel).ToString();
        }

        if (config.RotateUnlockLevel > curLevel+1) {
            _rotateImageButton.sprite = lockSprite;
            _rotatePieceButton.enabled = false;
            _rotatePieceCountText.text = (config.RotateUnlockLevel).ToString();
        }
    }

    public void UpdateCounters(GameDataForSave data) {
        if (_randomFieldButton.enabled) {
            _randomFieldCountText.text = data.ResourcesCount[ResourceType.ShuffleBooster].ToString();
        }

        if (_dinamyteButton.enabled) {
            _dinamyteCountText.text = data.ResourcesCount[ResourceType.BombBooster].ToString();
        }

        if (_hummerButton.enabled) {
            _hummerCountText.text = data.ResourcesCount[ResourceType.HammerBooster].ToString();
        }

        if (_rotatePieceButton.enabled) {
            _rotatePieceCountText.text = data.ResourcesCount[ResourceType.RotateBooster].ToString();
        }
    }
}