using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector3 startPosition, offset;
    public Vector3 kakanTo = Vector3.zero;
    public bool isDragging = false, moved = false;
    public bool canTouch = true;
    private readonly float smoothTime = 0.10f;
    private Coroutine moveCoroutine;
    public int place = -1;
    public int playerid;


    private float lastMouseDownTime = 0;

    private void OnMouseEnter()
    {
        if (!canTouch) return;
        GameManager.instance.playerManager.DisplayMachi(Library.ToInt(name));
    }

    private void OnMouseExit()
    {
        if (!canTouch) return;
        GameManager.instance.playerManager.ClearMachiDisplay();
    }

    private void OnMouseDown()
    {
        if (!canTouch) return;
        if (moved) return;
        startPosition = transform.position;
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;
        lastMouseDownTime = Time.time;
    }

    private void OnMouseDrag()
    {
        if (!canTouch) return;
        if (GameManager.instance.playerManager == null) return;
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
            if (place < GameManager.instance.playerManager.TileCount() && transform.position.x - GameManager.instance.playerManager.GetPosition(place).x > 0.51f)
            {
                GameManager.instance.playerManager.SwapTiles(place, 1);
            }
            else if (place > 0 && transform.position.x - GameManager.instance.playerManager.GetPosition(place).x < -0.51f)
            {
                GameManager.instance.playerManager.SwapTiles(place, -1);
            }
        }
    }

    private void OnMouseUp()
    {
        if (!canTouch ) return;
        isDragging = false;
        if (!moved && (GameManager.instance.activePlayerId == playerid /* || GameManager.instance.furoNow[playerid] */) && (Time.time - lastMouseDownTime<= 0.2f || Mathf.Abs(startPosition.y - transform.position.y) > 1))
        {
            GameManager.instance.playerManager.riverCount++;
            if (kakanTo != Vector3.zero)
            {
                MoveTo(kakanTo, true);
                moved = true;
                GameManager.instance.playerManager.DiscardTile(this);
                canTouch = false;
                return;
            }
            if (GameManager.instance.playerManager.id == GameManager.instance.activePlayerId && GameManager.instance.playerManager.turnCount == GameManager.instance.playerManager.riichiTurn)
            {
                MoveTo(GameManager.instance.playerManager.riverTail + new Vector3(0.12f, 0.12f, 0), true); // trueÇ≈âÒì]ÇìKóp
                if (GameManager.instance.playerManager.riverCount % 6 == 0)
                {
                    GameManager.instance.playerManager.riverTail = GameManager.instance.playerManager.firstRiverTail + new Vector3(0, - (GameManager.instance.playerManager.riverCount / 6) * 0.9f, 0);
                }
                else
                {
                    GameManager.instance.playerManager.riverTail += new Vector3(0.9f, 0, 0);
                }
            }
            else
            {
                MoveTo(GameManager.instance.playerManager.riverTail, true);
                if (GameManager.instance.playerManager.riverCount % 6 == 0)
                {
                    GameManager.instance.playerManager.riverTail = GameManager.instance.playerManager.firstRiverTail + new Vector3(0, -(GameManager.instance.playerManager.riverCount / 6) * 0.9f, 0);
                }
                else
                {
                    GameManager.instance.playerManager.riverTail += new Vector3(0.66f, 0, 0);
                }
            }
            moved = true;
            this.transform.localScale = Vector3.one;
            GameManager.instance.playerManager.panelManager.river[GameManager.instance.playerManager.id].Add(this.transform);
            GameManager.instance.playerManager.OnClickDiscard(this);
            canTouch = false;
        }
        else
        {
            MoveTo(GameManager.instance.playerManager.firstHandPlace + new Vector3(place, 0, 0), false);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    // âÒì]Çà¯êîÇ≈êßå‰
    public void MoveTo(Vector3 targetPosition, bool rotate)
    {
        if (!canTouch && GameManager.instance.playerManager.riichiTurn == -1) return;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMove(targetPosition, rotate));
    }

    private IEnumerator SmoothMove(Vector3 target, bool rotate)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation; 
        if (GameManager.instance.playerManager == null) targetRotation = transform.rotation;
        else
        {
            targetRotation = (rotate && GameManager.instance.activePlayerId == GameManager.instance.playerManager.id &&  GameManager.instance.playerManager.turnCount == GameManager.instance.playerManager.riichiTurn) ? Quaternion.Euler(0, 0, 90) : transform.rotation; // âÒì]èàóùÇà¯êîÇ≈êßå‰
        }

        while (elapsed < smoothTime)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / smoothTime);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / smoothTime);
            elapsed += Mathf.Max(Time.deltaTime, 0.001f); // ç≈è¨ílÇê›íË
            yield return null;
        }

        transform.position = target;
        transform.rotation = targetRotation; // ç≈å„Ç…âÒì]Çämé¿Ç…ìKóp
    }
}
