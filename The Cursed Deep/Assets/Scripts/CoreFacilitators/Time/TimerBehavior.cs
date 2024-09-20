using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimerBehavior : MonoBehaviour
{
    public UnityEvent updateTextEvent;
    
    [SerializeField] private BoolData canRunTimer;
    [SerializeField] private FloatData timer;

    private float _seconds =  0.01f;
    private float _elapsedTime;
    private WaitForSecondsRealtime _wfsrtObj;

    private void Start()
    {
        _wfsrtObj = new WaitForSecondsRealtime(_seconds);
    }
    
    public void StartTimer()
    {
        StartCoroutine(UpdateTimer());
    }    
    
    public void StopTimer()
    {
        StopCoroutine(UpdateTimer());
    }
    
    private IEnumerator UpdateTimer()
    {
        while (canRunTimer)
        {
            _elapsedTime += Time.deltaTime;
            timer.value = _elapsedTime;
            updateTextEvent.Invoke();
            yield return null;
        }
    }
}
