using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TrailMakingManager : MonoBehaviour
{
    public enum TestPart { A, B }

    public static TrailMakingManager Instance;

    public TestPart part = TestPart.A;
    public Waypoint[] routeTutorial;
    public Waypoint[] routeA;
    public Waypoint[] routeB;
    public Transform plane;
    public LineRenderer trail;

	public Vector2 planeStartOffset = new Vector2(0f, -4f);
    public Vector2 tutorialPlaneStart = new Vector2(-8.05f, -2.61f);
    public GameObject tutorialObjects;
    public GameObject partAObjects;
    public GameObject partBObjects;
	public GameObject startPanel;
    public GameObject tutorialDonePanel;
    public GameObject readyPanel;
    public GameObject messagePanel;

    public TMP_Text resultText;
    public TMP_Text timerText;
    public TMP_Text errorCountText;

	public float nextSceneDelay = 3f;
    Waypoint[] route;
    int currentIndex;
    int errors;
    float startTime;
    bool finished;
    bool inTutorial;
    Waypoint lastWaypointOver;

    float timeA;
    int errorsA;
    float timeB;
    int errorsB;

    void Awake()
    {
        Instance = this;
        TrailMakingResults.Clear();
    }

    void Update()
    {
        if (!finished && timerText != null)
        {
            float elapsed = Time.time - startTime;
            timerText.text = "Time: " + elapsed.ToString("F1") + "s";
        }
    }

    void UpdateErrorDisplay()
    {
        if (errorCountText != null)
        {
            errorCountText.text = "Errors: " + errors;
        }
    }
    
	void Start()
	{
    	if (partBObjects != null) partBObjects.SetActive(false);
    	if (readyPanel != null) readyPanel.SetActive(false);
    	if (tutorialDonePanel != null) tutorialDonePanel.SetActive(false);

    	finished = true;

    	if (startPanel != null) startPanel.SetActive(true);
    	else StartTutorial();
	}

    void SetupRound()
    {
        errors = 0;
        finished = false;
        lastWaypointOver = null;

        for (int i = 0; i < route.Length; i++)
        {
            route[i].order = i;
            route[i].SetLabel(GetLabel(i));
            route[i].ResetColor();
        }

        if (inTutorial)
        {
            plane.position = tutorialPlaneStart;
            currentIndex = 0;
        }
        else
        {
            plane.position = route[0].transform.position + (Vector3)planeStartOffset;
            route[0].MarkVisited();
            currentIndex = 1;
        }

        trail.positionCount = 1;
        trail.SetPosition(0, plane.position);

        startTime = Time.time;

        if (resultText != null) resultText.text = "";
        if (messagePanel != null) messagePanel.SetActive(false);
        UpdateErrorDisplay();
    }

	public void OnStartClicked()
	{
    	if (startPanel != null) startPanel.SetActive(false);
    	StartTutorial();
	}

    void StartTutorial()
    {
        if (routeTutorial == null || routeTutorial.Length == 0)
        {
            StartPartA();
            return;
        }

        inTutorial = true;
        part = TestPart.A;
        route = routeTutorial;

        if (tutorialObjects != null) tutorialObjects.SetActive(true);
        if (partAObjects != null) partAObjects.SetActive(false);
        if (partBObjects != null) partBObjects.SetActive(false);

        if (timerText != null) timerText.gameObject.SetActive(false);
        if (errorCountText != null) errorCountText.gameObject.SetActive(false);

        SetupRound();
    }

    public void OnTutorialDoneClicked()
    {
        if (tutorialDonePanel != null) tutorialDonePanel.SetActive(false);
        StartPartA();
    }

    void StartPartA()
    {
        inTutorial = false;
        part = TestPart.A;
        route = routeA;

        if (tutorialObjects != null) tutorialObjects.SetActive(false);
        if (partAObjects != null) partAObjects.SetActive(true);
        if (partBObjects != null) partBObjects.SetActive(false);

        if (timerText != null) timerText.gameObject.SetActive(true);
        if (errorCountText != null) errorCountText.gameObject.SetActive(true);

        SetupRound();
    }

    public void OnReadyClicked()
	{
    	if (readyPanel != null) readyPanel.SetActive(false);
    	if (partAObjects != null) partAObjects.SetActive(false);
    	if (partBObjects != null) partBObjects.SetActive(true);

    	if (timerText != null) timerText.gameObject.SetActive(true);
    	if (errorCountText != null) errorCountText.gameObject.SetActive(true);

    	part = TestPart.B;
    	route = routeB;
    	SetupRound();
	}

    public void Draw(Vector3 penPos)
    {
        if (finished) return;

        AddTrailPoint(penPos);
        CheckWaypoint(penPos);
    }

    void AddTrailPoint(Vector3 penPos)
    {
        Vector3 last = trail.GetPosition(trail.positionCount - 1);
        if (Vector3.Distance(last, penPos) < 0.1f)
        {
            return;
        }

        trail.positionCount++;
        trail.SetPosition(trail.positionCount - 1, penPos);
    }

    void CheckWaypoint(Vector3 penPos)
    {
        Waypoint over = GetWaypointAt(penPos);

        if (over == lastWaypointOver)
        {
            return;
        }
        lastWaypointOver = over;

        if (over == null)
        {
            return;
        }

        if (over.order == currentIndex)
        {
            Connect(over);
        }
        else if (over.order > currentIndex)
        {
            errors++;
            UpdateErrorDisplay();
            if (messagePanel != null)
            {
                messagePanel.SetActive(true);
            }
        }
    }

    void Connect(Waypoint w)
    {
        w.MarkVisited();
        currentIndex++;

        if (messagePanel != null) messagePanel.SetActive(false);

        if (currentIndex >= route.Length)
        {
            FinishRound();
        }
    }

    void FinishRound()
    {
        finished = true;
        float total = Time.time - startTime;

        if (timerText != null) timerText.text = "Time: " + total.ToString("F1") + "s";

        if (inTutorial)
        {
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (errorCountText != null) errorCountText.gameObject.SetActive(false);
            if (tutorialObjects != null) tutorialObjects.SetActive(false);
            if (tutorialDonePanel != null) tutorialDonePanel.SetActive(true);
            else StartPartA();
            return;
        }

        if (part == TestPart.A)
        {
            timeA = total;
            errorsA = errors;
            TrailMakingResults.Record("A", timeA, errorsA);
            if (readyPanel != null) readyPanel.SetActive(true);
			if (timerText != null) timerText.gameObject.SetActive(false);
    		if (errorCountText != null) errorCountText.gameObject.SetActive(false);
        }
        else
		{
    		timeB = total;
    		errorsB = errors;
    		TrailMakingResults.Record("B", timeB, errorsB);
    		TrailMakingResults.LogResults();
    		ShowResults();
    		Invoke(nameof(GoToCargo), nextSceneDelay);
		}
    }

    void ShowResults()
    {
        if (resultText != null)
        {
            float difference = timeB - timeA;
            float ratio = timeA > 0f ? timeB / timeA : 0f;

            resultText.text =
                "Part A: " + timeA.ToString("F1") + "s, " + errorsA + " errors\n" +
                "Part B: " + timeB.ToString("F1") + "s, " + errorsB + " errors\n\n" +
                "B - A: " + difference.ToString("F1") + "s\n" +
                "B / A: " + ratio.ToString("F2");
        }
    }

    Waypoint GetWaypointAt(Vector2 pos)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(pos);

        for (int i = 0; i < hits.Length; i++)
        {
            Waypoint w = hits[i].GetComponent<Waypoint>();
            if (w != null)
            {
                return w;
            }
        }

        return null;
    }

    string GetLabel(int index)
    {
        if (part == TestPart.A)
        {
            return (index + 1).ToString();
        }

        if (index % 2 == 0)
        {
            return (index / 2 + 1).ToString();
        }
        else
        {
            char letter = (char)('A' + index / 2);
            if (letter == 'I')
            {
                return "i";
            }
            return letter.ToString();
        }
    }

	void GoToCargo()
	{
    	SceneManager.LoadScene("cargoGame");
	}
}