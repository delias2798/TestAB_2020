using TMPro;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class TriviaGame : MonoBehaviour
{
    public GameObject Canvas;
    public GameObject PanelStart;
    public GameObject PanelEnd;
    public GameObject PanelPrefab;
    public GameObject buttonPrefab;
    private GameObject Panel;
    private int currentPanelIndex = 0;
    private List<GameObject> panels = new List<GameObject>();
    private string filePath;
    private int corrects;
    private int wrongs;
    private string[] answersList;
    private List<string> list;

    public UnityEvent TriviStarEvent;
    public UnityEvent TriviaEndEvent;
    public Animator manateeAnimator;

    void Start()
    {
        // string jsonPath = "trivia.json";
        // string jsonString = BetterStreamingAssets.ReadAllText(jsonPath);
        list = new List<string> {};

        string jsonString;
        string jsonPath = Path.Combine(Application.streamingAssetsPath, "trivia.json");
        // filePath = Path.Combine(Application.streamingAssetsPath, "results.json");
        filePath = Path.Combine(Application.persistentDataPath, "results.json");


        if (File.Exists(jsonPath))
        {
            jsonString = File.ReadAllText(jsonPath);
        }
        else
        {
            Debug.LogError("File not found: " + jsonPath);
            return;
        }

        Trivia trivia = JsonUtility.FromJson<Trivia>(jsonString);

        TextMeshProUGUI panelStartText = PanelStart.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI panelEndText = PanelEnd.GetComponentInChildren<TextMeshProUGUI>();
        Button startButton = PanelStart.GetComponentInChildren<Button>();
        Button endButton = PanelEnd.GetComponentInChildren<Button>();
        TextMeshProUGUI startButtonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI endButtonText = endButton.GetComponentInChildren<TextMeshProUGUI>();

        panelStartText.text = trivia.tittle;
        panelEndText.text = trivia.endTittle;
        startButtonText.text = trivia.startButtonName;
        endButtonText.text = trivia.endButtonName;

        startButton.onClick.AddListener(() => startTrivia());
        endButton.onClick.AddListener(() => endTrivia());

        PanelStart.SetActive(true);
        PanelEnd.SetActive(false);


        foreach (Question question in trivia.questionList)
        {
            Panel = Instantiate(PanelPrefab, Canvas.transform, false);
            panels.Add(Panel);
            Button[] buttonsList = Panel.GetComponentsInChildren<Button>();
            TextMeshProUGUI panelText = Panel.GetComponentInChildren<TextMeshProUGUI>();
            
            panelText.text = question.question;

            CreateOptionButton(question.option1, 0, question.correct, buttonsList[0]);
            CreateOptionButton(question.option2, 1, question.correct, buttonsList[1]);
            CreateOptionButton(question.option3, 2, question.correct, buttonsList[2]);

            // if (panels.Count > 1)
            // {
            //     Panel.SetActive(false);
            // }
            Panel.SetActive(false);
        }
    }

    void CreateOptionButton(string optionText, int optionNumber, int correctAnswer, Button button)
    {
        button.gameObject.name = "Option" + (optionNumber + 1) + "Button";

        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = optionText;

        button.onClick.AddListener(() => CheckAnswer(correctAnswer, optionNumber));
    }

    public void startTrivia()
    {
        PanelStart.SetActive(false);
        panels[0].SetActive(true);
    }

    public void endTrivia()
    {
        PanelEnd.SetActive(false);
        TriviaEndEvent.Invoke();
        Debug.Log("Trivia completed!");
    }
    public void CheckAnswer(int correctAnswer, int userAnswer)
    {
        if (correctAnswer == userAnswer)
        {
            Debug.Log("Correct!");
            corrects++;
            list.Add("correct");
            manateeAnimator.SetTrigger("YesTrigger");
        }
        else
        {
            wrongs++;
            Debug.Log("Incorrect!");
            list.Add("incorrect");
            manateeAnimator.SetTrigger("NoTrigger");
        }

        panels[currentPanelIndex].SetActive(false);

        currentPanelIndex++;
        if (currentPanelIndex < panels.Count)
        {
            panels[currentPanelIndex].SetActive(true);
        }
        else
        {
            TextMeshProUGUI panelEndText = PanelEnd.GetComponentInChildren<TextMeshProUGUI>();
            panelEndText.text += "\nCorrect Answers: " + corrects;
            PanelEnd.SetActive(true);
            SaveResults();
        }
    }

    void SaveResults()
    {
        Results results;
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            results = JsonUtility.FromJson<Results>(json);
        }
        else
        {
            results = new Results();
            results.repliesList = new List<Replies>();
        }

        Replies replies = new Replies();
        replies.corrects = corrects;
        replies.wrongs = wrongs;
        replies.repliesRecord = list;

        results.totalCorrect += corrects;
        results.totalWrong += wrongs;
        results.repliesList.Add(replies);
        
        string updatedJson = JsonUtility.ToJson(results);
        File.WriteAllText(filePath, updatedJson);
        Debug.Log("Results saved to: " + filePath);
    }

    [Serializable]
    public class Trivia
    {
        public string tittle;
        public string startButtonName;
        public string endTittle;
        public string endButtonName;
        public List<Question> questionList;
    }

    [Serializable]
    public class Question
    {
        public string question;
        public string option1;
        public string option2;
        public string option3;
        public int correct;
    }

    [Serializable]
    public class Results
    {
        public int totalCorrect;
        public int totalWrong;
        public List<Replies> repliesList;
    }

    [Serializable]
    public class Replies
    {
        public int corrects;
        public int wrongs;
        public List<string> repliesRecord;
    }
}
