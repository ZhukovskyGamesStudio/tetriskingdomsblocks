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
    private Button _dinamyteButton;

    [SerializeField]
    private Button _hummerButton;

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

        if (config.RandomUnlockLevel > curLevel) {
            _randomImageButton.sprite = lockSprite;
            _randomFieldButton.enabled = false;
            _randomFieldCountText.text = (config.RandomUnlockLevel + 1).ToString();
        }

        if (config.DynamiteUnlockLevel > curLevel) {
            _dynamiteImageButton.sprite = lockSprite;
            _dinamyteButton.enabled = false;
            _dinamyteCountText.text = (config.DynamiteUnlockLevel + 1).ToString();
        }

        if (config.HummerUnlockLevel > curLevel) {
            _hummerImageButton.sprite = lockSprite;
            _hummerButton.enabled = false;
            _hummerCountText.text = (config.HummerUnlockLevel + 1).ToString();
        }

        if (config.RotateUnlockLevel > curLevel) {
            _rotateImageButton.sprite = lockSprite;
            _rotatePieceButton.enabled = false;
            _rotatePieceCountText.text = (config.RotateUnlockLevel + 1).ToString();
        }
    }

    public void UpdateCounters(GameDataForSave data) {
        if (_randomFieldButton.enabled) {
            _randomFieldCountText.text = data.RandomFieldCount.ToString();
        }

        if (_dinamyteButton.enabled) {
            _dinamyteCountText.text = data.DynamiteCount.ToString();
        }

        if (_hummerButton.enabled) {
            _hummerCountText.text = data.HummerCount.ToString();
        }

        if (_rotatePieceButton.enabled) {
            _rotatePieceCountText.text = data.RotatePieceCount.ToString();
        }
    }
}