using UnityEngine;

public class CellController : MonoBehaviour
{
    public Sprite floorSprite;
    public Sprite stairsSprite;
    public Sprite[] crackSprites;
    public Sprite playerSprite;
    public Sprite heartSprite;
    public Sprite monsterSprite;
    public Sprite pathHSprite;
    public Sprite pathVSprite;
    public Sprite path00Sprite;
    public Sprite path01Sprite;
    public Sprite path10Sprite;
    public Sprite path11Sprite;

    public CellType type;
    public bool changed;
    public bool hasStairs;

    public SpriteRenderer floorRenderer;
    public SpriteRenderer cracksRenderer;
    public SpriteRenderer stairsRenderer;
    public SpriteRenderer itemRenderer;
    public SpriteRenderer itemShadowRenderer;

    private static readonly int[] _angleRnd = new int[] { 0, 90, 180, 270 };

    void Start()
    {
        cracksRenderer.enabled = false;// (Random.Range(0.0f, 1.0f) > 0.6f);
        cracksRenderer.sprite = crackSprites[Random.Range(0, crackSprites.Length)];
        cracksRenderer.transform.rotation = Quaternion.Euler(0, 0, _angleRnd[Random.Range(0, _angleRnd.Length)]);
    }

    void Update()
    {
        if (changed)
        {
            itemRenderer.enabled = false;
            itemShadowRenderer.enabled = false;
            
            if (hasStairs)
            {
                floorRenderer.sprite = stairsSprite;
            }
            else
            {
                floorRenderer.sprite = floorSprite;
            }

            switch (type)
            {
                case CellType.Block:
                {
                    floorRenderer.color = Color.grey;
                    break;
                }
                case CellType.PathH:
                {
                    itemRenderer.sprite = pathHSprite;
                    itemRenderer.enabled = true;
                    break;
                }
                case CellType.PathV:
                {
                    itemRenderer.sprite = pathVSprite;
                    itemRenderer.enabled = true;
                    break;
                }
                case CellType.Path00:
                {
                    itemRenderer.sprite = path00Sprite;
                    itemRenderer.enabled = true;
                    break;
                }
                case CellType.Path01:
                {
                    itemRenderer.sprite = path01Sprite;
                    itemRenderer.enabled = true;
                    break;
                }
                case CellType.Path10:
                {
                    itemRenderer.sprite = path10Sprite;
                    itemRenderer.enabled = true;
                    break;
                }
                case CellType.Path11:
                {
                    itemRenderer.sprite = path11Sprite;
                    itemRenderer.enabled = true;
                    break;
                }
                case CellType.Heart:
                {
                    itemRenderer.sprite = heartSprite;
                    itemRenderer.enabled = true;
                    itemShadowRenderer.enabled = true;
                    break;
                }
                case CellType.Monster:
                {
                    itemRenderer.sprite = monsterSprite;
                    itemRenderer.enabled = true;
                    itemShadowRenderer.enabled = true;
                    break;
                }
                case CellType.Player:
                {
                    itemRenderer.sprite = playerSprite;
                    itemRenderer.enabled = true;
                    itemShadowRenderer.enabled = true;
                    break;
                }
            }

            changed = false;
        }
    }
}
