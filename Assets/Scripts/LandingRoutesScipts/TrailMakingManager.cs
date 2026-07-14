using UnityEngine;
using TMPro;

public class TrailMakingManager : MonoBehaviour
{
    public enum TestPart { A, B }

    public static TrailMakingManager Instance;

    public TestPart part = TestPart.A;
    public Waypoint[] routeA;
    public Waypoint[] routeB;
    public Transform plane;
    public LineRenderer trail;

	public Vector2 planeStartOffset = new Vector2(0f, -4f);
    public GameObject partAObjects;
    public GameObject partBObjects;
    public GameObject readyPanel;

    public TMP_Text resultText;
    public TMP_Text messageText;

    Waypoint[] route;
    int currentIndex;
    int errors;
    float startTime;
    bool finished;
    Waypoint lastWaypointOver;

    float timeA;
    int errorsA;
    float timeB;
    int errorsB;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (partBObjects != null) partBObjects.SetActive(false);
        if (readyPanel != null) readyPanel.SetActive(false);

        part = TestPart.A;
        route = routeA;
        SetupRound();
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

        plane.position = route[0].transform.position + (Vector3)planeStartOffset;
        route[0].MarkVisited();
        currentIndex = 1;

        trail.positionCount = 1;
        trail.SetPosition(0, route[0].transform.position);

        startTime = Time.time;

        if (resultText != null) resultText.text = "";
        if (messageText != null) messageText.text = "";
    }

    public void OnReadyClicked()
    {
        if (readyPanel != null) readyPanel.SetActive(false);
        if (partAObjects != null) partAObjects.SetActive(false);
        if (partBObjects != null) partBObjects.SetActive(true);

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
            if (messageText != null)
            {
                messageText.text = "Error! Where should you go from here?";
            }
        }
    }

    void Connect(Waypoint w)
    {
        w.MarkVisited();
        currentIndex++;

        if (messageText != null) messageText.text = "";

        if (currentIndex >= route.Length)
        {
            FinishRound();
        }
    }

    void FinishRound()
    {
        finished = true;
        float total = Time.time - startTime;

        if (part == TestPart.A)
        {
            timeA = total;
            errorsA = errors;
            if (readyPanel != null) readyPanel.SetActive(true);
        }
        else
        {
            timeB = total;
            errorsB = errors;
            ShowResults();
        }
    }

    void ShowResults()
    {
        if (resultText != null)
        {
            resultText.text =
                "Part A: " + timeA.ToString("F1") + "s, " + errorsA + " errors\n" +
                "Part B: " + timeB.ToString("F1") + "s, " + errorsB + " errors";
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
            return ((char)('A' + index / 2)).ToString();
        }
    }
}