using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Vector3 startPosition, offset;
    public Vector3 kakanTo = Vector3.zero;
    private bool isDragging = false, moved = false;
    public bool canTouch = true;
    private readonly float smoothTime = 0.05f;
    private Coroutine moveCoroutine;
    public int place = -1;


    private float lastMouseDownTime = 0;

    private void OnMouseEnter()
    {
        if (!canTouch) return;
        PlayerManager.instance.DisplayMachi(Library.ToInt(name));
    }

    private void OnMouseExit()
    {
        if (!canTouch) return;
        PlayerManager.instance.ClearMachiDisplay();
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
        if (PlayerManager.instance == null) return;
        if (isDragging)
        {
            transform.position = GetMouseWorldPosition() + offset;
            if (place < PlayerManager.instance.TileCount() && transform.position.x - PlayerManager.instance.GetPosition(place).x > 0.51f)
            {
                PlayerManager.instance.SwapTiles(place, 1);
            }
            else if (place > 0 && transform.position.x - PlayerManager.instance.GetPosition(place).x < -0.51f)
            {
                PlayerManager.instance.SwapTiles(place, -1);
            }
        }
    }

    private void OnMouseUp()
    {
        if (!canTouch ) return;
        isDragging = false;
        if (!moved && PlayerManager.instance.ItsMyTurn() && (Time.time - lastMouseDownTime<= 0.2f || Mathf.Abs(startPosition.y - transform.position.y) > 1))
        {
            if (PlayerManager.instance == null) return;
            PlayerManager.instance.riverCount++;
            if (kakanTo != Vector3.zero)
            {
                MoveTo(kakanTo, true);
                moved = true;
                PlayerManager.instance.DiscardTile(this);
                canTouch = false;
                return;
            }
            if (PlayerManager.instance.riichiedJustNow >= 1)
            {
                MoveTo(PlayerManager.instance.riverTail + new Vector3(0.12f, 0.12f, 0), true); // trueÇ≈âÒì]ÇìKóp
                if (PlayerManager.instance.riverCount % 6 == 0)
                {
                    PlayerManager.instance.riverTail = PlayerManager.instance.firstRiverTail + new Vector3(0, - (PlayerManager.instance.riverCount / 6) * 0.9f, 0);
                }
                else
                {
                    PlayerManager.instance.riverTail += new Vector3(0.9f, 0, 0);
                }
            }
            else
            {
                MoveTo(PlayerManager.instance.riverTail, true);
                if (PlayerManager.instance.riverCount % 6 == 0)
                {
                    PlayerManager.instance.riverTail = PlayerManager.instance.firstRiverTail + new Vector3(0, -(PlayerManager.instance.riverCount / 6) * 0.9f, 0);
                }
                else
                {
                    PlayerManager.instance.riverTail += new Vector3(0.66f, 0, 0);
                }
            }
            moved = true;
            this.transform.localScale = Vector3.one;
            PlayerManager.instance.DiscardTile(this);
            canTouch = false;
        }
        else
        {
            MoveTo(PlayerManager.instance.firstHandPlace + new Vector3(place, 0, 0), false);
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
        if (!canTouch) return;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(SmoothMove(targetPosition, rotate));
    }

    private IEnumerator SmoothMove(Vector3 target, bool rotate)
    {
        float elapsed = 0f;
        Vector3 start = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation; 
        if (PlayerManager.instance == null) targetRotation = transform.rotation;
        else
        {
            PlayerManager.instance.riichiedJustNow--;
            targetRotation = (rotate && PlayerManager.instance.riichiedJustNow >= 1) ? Quaternion.Euler(0, 0, 90) : transform.rotation; // âÒì]èàóùÇà¯êîÇ≈êßå‰
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
