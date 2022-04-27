using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Toggle_Switch : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private bool _isOn = false;
    public bool isOn
    {
        get
        {
            return _isOn;
        }
    } 

    [SerializeField]
    private RectTransform toggleIndicator;
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Color onColor;
    [SerializeField]
    private Color offColor;
    private float offx;
    private float onx;
    [SerializeField]
    private float TweenTime = 0.25f;

    public delegate void ValueChanged(bool value);
    public event ValueChanged valueChanged;


    // Start is called before the first frame update
    void Start()
    {
        offx = toggleIndicator.anchoredPosition.y;
        onx = backgroundImage.rectTransform.rect.height - 2*toggleIndicator.rect.width;

    }

    private void OnEnable()
    {
        Toggle(isOn);
    }

    // Update is called once per frame
    private void Toggle(bool value)
    {
        if (value!=isOn){
            _isOn = value;
            ToggleColor(isOn);
            MoveIndicator(isOn);
            
            if(valueChanged != null)
            {
                valueChanged(isOn);
            }
        }
    }
    
    private void ToggleColor(bool value)
    {
        if (value)
        {
            backgroundImage.DOColor(onColor, TweenTime);
        }
        else
        {
            backgroundImage.DOColor(offColor, TweenTime);
        }
    }

    private void MoveIndicator(bool value)
    {
        if (value)
        {
            toggleIndicator.DOAnchorPosY(onx, TweenTime);
        }
        else
        {
            toggleIndicator.DOAnchorPosY(offx, TweenTime);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Toggle(!isOn);
    }
}
 