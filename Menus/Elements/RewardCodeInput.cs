using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;

public class RewardCodeInput : MonoBehaviour
{
  public InputField codeInputField;
  public Text resultText;

  private DatabaseReference databaseReference;

  void Start()
  {
    // Initialize Firebase Database reference
    databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
  }

  public void CheckRewardCode()
  {
    string inputCode = codeInputField.text;

    databaseReference.Child("codes").GetValueAsync().ContinueWithOnMainThread(task =>
    {
      if (task.IsFaulted)
      {
        resultText.text = "Error checking code.";
      }
      else if (task.IsCompleted)
      {
        DataSnapshot snapshot = task.Result;
        bool codeExists = false;

        foreach (DataSnapshot codeSnapshot in snapshot.Children)
        {
          if (codeSnapshot.Child("code").Value.ToString() == inputCode)
          {
            codeExists = true;
            break;
          }
        }

        if (codeExists)
        {
          resultText.text = "Code is valid!";
        }
        else
        {
          resultText.text = "Invalid code.";
        }
      }
    });
  }
}