using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SocialMediaButton : MonoBehaviour
{
  public SocialMediaLinks socialMediaLinks;
  public enum SocialMediaType { Facebook, Twitter, Instagram, YouTube, TikTok }
  public SocialMediaType socialMediaType;
  private Button button;

  private void Awake()
  {
    button = GetComponent<Button>();
    button.onClick.AddListener(OnButtonClick);
  }

  public void OnButtonClick()
  {
    string url = "";

    switch (socialMediaType)
    {
      case SocialMediaType.Facebook:
        url = socialMediaLinks.facebookUrl;
        break;
      case SocialMediaType.Twitter:
        url = socialMediaLinks.twitterUrl;
        break;
      case SocialMediaType.Instagram:
        url = socialMediaLinks.instagramUrl;
        break;
      case SocialMediaType.YouTube:
        url = socialMediaLinks.youtubeUrl;
        break;
      case SocialMediaType.TikTok:
        url = socialMediaLinks.tiktokUrl;
        break;
    }

    if (!string.IsNullOrEmpty(url))
    {
      Application.OpenURL(url);
    }
    else
    {
      Debug.LogWarning("URL is not set for " + socialMediaType.ToString());
    }
  }
}