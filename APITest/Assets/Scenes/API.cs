using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
using UnityEngine.SceneManagement;

public class API : MonoBehaviour
{
    
    public Text upc;
    public string S_UPC;
    public string GTP;
    public string[] AddedSugar = new string [247];
    //036632008398  791083622813 036632008787
    private const string URL = "https://api.nal.usda.gov/ndb/search/?format=json&q=";
    private const string API_KEY = "kljZyOcE6L8Ml6Z5PJeNvYOcLib9C4NaQgEl4EBB";
    public Text responseText;
    public Text DeckText;
    public string jsonstring;
    int count = 0;
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
           var dependencyStatus = task.Result;
           if (dependencyStatus == Firebase.DependencyStatus.Available)
           {
               // Create and hold a reference to your FirebaseApp, i.e.
               //   app = Firebase.FirebaseApp.DefaultInstance;
               // where app is a Firebase.FirebaseApp property of your application class.

               // Set a flag here indicating that Firebase is ready to use by your
               // application.
           }
           else
           {
               UnityEngine.Debug.LogError(System.String.Format(
                 "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
               // Firebase Unity SDK is not safe to use here.
           }
       });
        // Set this before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://sugarmon-5e94f.firebaseio.com/");
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;

       
           responseText.text = "Enter UPC Number";
               
           print(responseText.text);
     

    }
    public void Request()
    {
        S_UPC = upc.text;
        WWW request = new WWW(URL +  S_UPC + "&" + "api_key=" + API_KEY);
        StartCoroutine(OnResponse(request));
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // loads current scene
    }


    private IEnumerator OnResponse(WWW req)
    {

        yield return req;

      

        if ( count == 0)
        {
            GTP = req.text;
           S_UPC = "00" + S_UPC;
          WWW request = new WWW(URL + S_UPC + "&" + "api_key=" + API_KEY);
         StartCoroutine(OnResponse(request));
           count++;
       }
        if(GTP.Length > req.text.Length)
        {
            jsonstring = GTP;

            Food food = JsonUtility.FromJson<Food>(jsonstring);

            string ndbno = food.list.item[0].ndbno; //food.list.item.ndbno.ToString();

            WWW newrequest = new WWW("https://api.nal.usda.gov/ndb/reports/?ndbno=" + ndbno + "&type=f&format=json&api_key=MdQT6uuqMt4epBN9IAGOO1qSPH9GhMmIdJLwVlNP"); StartCoroutine(OnnewResponse(newrequest));
        }
        else if (GTP.Length < req.text.Length)
        {
            jsonstring = req.text;

            Food food = JsonUtility.FromJson<Food>(jsonstring);

            string ndbno = food.list.item[0].ndbno;

            WWW newrequest = new WWW("https://api.nal.usda.gov/ndb/reports/?ndbno=" + ndbno + "&type=f&format=json&api_key=MdQT6uuqMt4epBN9IAGOO1qSPH9GhMmIdJLwVlNP"); StartCoroutine(OnnewResponse(newrequest));
        }
        else
        {
            responseText.text = "UPC not Found";
            print(responseText.text);
            DeckText.text = "N/A";
            print(DeckText.text);
            
        }


      
}
private IEnumerator OnnewResponse(WWW req)
{

    yield return req;

      
        jsonstring = req.text;
        FoodDesc thefood = JsonUtility.FromJson<FoodDesc>(jsonstring);
        int AS = 0;
        string ASname = "";
        string DASnum = "";
        string b = "";
        string list = "";
        string Decklist = " ";
        for (int i = 1; i < 248; i++)
        {
            b = i.ToString();
            FirebaseDatabase.DefaultInstance
                            .GetReference("/AddedSugar/" +b)
     .GetValueAsync().ContinueWith(task => {
         
                ASname = task.Result.Child("/Name").Value.ToString();

              
                if(list.Length < 3)
                {
                    responseText.text = "No Sugar found"; 
             print(responseText.text);
                    DeckText.text = "N/A";
             print(DeckText.text);
                }

         if (thefood.report.food.ing.desc.ToUpper().Contains(ASname.ToUpper()))
         {
             AddedSugar[AS] = ASname;
             AS++;
            list = list + " " + ASname + ",";
            responseText.text = list; 
             print(responseText.text);
             DASnum = task.Result.Child("/Value").Value.ToString();
             Decklist = Decklist + " " + DASnum + ",";
             DeckText.text = Decklist; 
             print(DeckText.text);

         }
        
     });
             
        }


}
}
[System.Serializable]
public class Food
{
    public List list;

}
[System.Serializable]
public class List
{
    public string q;
    public string sr;
    public string ds { get; set; }
    public int start { get; set; }
    public int end { get; set; }
    public int total { get; set; }
    public string group { get; set; }
    public string sort { get; set; }
    public Items[] item;
}
[System.Serializable]
public class FoodDesc
{
    public Report report;

}
[System.Serializable]
public class Report
{
    public string sr;
    public Thefoods food;
}
[System.Serializable]
public class Thefoods
{
    public Ing ing;

}

[System.Serializable]
public class Ing
{
    public string desc;

}


[System.Serializable]
public class Items
{
    public int offset;
    public string group;
    public string name;
    public string ndbno;
    public string ds;
    public string manu;
   
}
