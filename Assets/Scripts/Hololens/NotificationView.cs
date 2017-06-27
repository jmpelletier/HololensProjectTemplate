using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Canvas))]
public class NotificationView : MonoBehaviour {

    public static InstanceManager<NotificationView> instances = new InstanceManager<NotificationView>();

    public float displayTime = 3f;

    private Text label;
    private Image panel;

    public static void Print(string message)
    {
        instances.main.Notify(message);
    }

    private void Awake () {
        instances.Add(this);
	}

    private void Start()
    {
        label = GetComponentInChildren<Text>();
        panel = GetComponentInChildren<Image>();
        panel.gameObject.GetComponent<CanvasRenderer>().SetAlpha(0);
        gameObject.SetActive(false);
    }

    public void Notify(string message)
    {
        Notify(message, displayTime);
    }

    public void Notify(string message, float time)
    {
        label.text = message;
        Show();
        StartCoroutine(EndNotification(time));
    }

    private IEnumerator EndNotification(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }

    private IEnumerator setActiveTimer;
    private IEnumerator SetActiveLater(bool state, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(state);
        setActiveTimer = null;
    }

    public void Show()
    {
        if(setActiveTimer != null)
        {
            StopCoroutine(setActiveTimer);
            setActiveTimer = null;
        }
        gameObject.SetActive(true);
        panel.CrossFadeAlpha(1f, 0.3f, false);
        label.CrossFadeAlpha(1f, 0.3f, false);
    }

    public void Hide()
    {
        panel.CrossFadeAlpha(0f, 0.3f, false);
        label.CrossFadeAlpha(0f, 0.3f, false);
        setActiveTimer = SetActiveLater(false, 0.3f);
        StartCoroutine(setActiveTimer);
    }
}
