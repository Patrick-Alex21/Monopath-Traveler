using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ScrollingBG : MonoBehaviour
{
    private RawImage mapImage;
    
    [Header("Scroll Settings")]
    public float maxSpeed = 0.03f;
    public float changeInterval = 4f;
    public float transitionSmoothness = 1.5f;

    private Vector2 currentSpeed;
    private Vector2 targetSpeed;
    private float timer;

    private int lastSignX = 0;
    private int countX = 0;
    
    private int lastSignY = 0;
    private int countY = 0;

    void Start()
    {
        mapImage = GetComponent<RawImage>();
        PickNewDirection();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            PickNewDirection();
            timer = 0f;
        }

        currentSpeed = Vector2.Lerp(currentSpeed, targetSpeed, Time.deltaTime * transitionSmoothness);

        Rect currentUV = mapImage.uvRect;
        
        float nextX = currentUV.x + (currentSpeed.x * Time.deltaTime);
        float nextY = currentUV.y + (currentSpeed.y * Time.deltaTime);

        float maxX = Mathf.Max(0f, 1f - currentUV.width);
        float maxY = Mathf.Max(0f, 1f - currentUV.height);

        if (nextX < 0f || nextX > maxX)
        {
            currentSpeed.x *= -1f; 
            targetSpeed.x *= -1f; 
            nextX = Mathf.Clamp(nextX, 0f, maxX); 
            
            lastSignX = (nextX <= 0f) ? 1 : -1;
            countX = 1;
        }

        if (nextY < 0f || nextY > maxY)
        {
            currentSpeed.y *= -1f;
            targetSpeed.y *= -1f;
            nextY = Mathf.Clamp(nextY, 0f, maxY);
            
            lastSignY = (nextY <= 0f) ? 1 : -1;
            countY = 1;
        }

        currentUV.x = nextX;
        currentUV.y = nextY;
        mapImage.uvRect = currentUV;
    }

    void PickNewDirection()
    {
        targetSpeed = new Vector2(
            CalculateAxisVelocity(ref lastSignX, ref countX),
            CalculateAxisVelocity(ref lastSignY, ref countY)
        );
    }

    float CalculateAxisVelocity(ref int lastSign, ref int count)
    {
        float randomVel;
        
        if (count >= 3)
        {
            lastSign = -lastSign; 
            
            randomVel = Random.Range(maxSpeed * 0.1f, maxSpeed) * lastSign;
            count = 1; 
        }
        else
        {
            randomVel = Random.Range(-maxSpeed, maxSpeed);
            int currentSign = (randomVel >= 0) ? 1 : -1;
            
            if (currentSign == lastSign || lastSign == 0)
            {
                count++; 
            }
            else
            {
                count = 1; 
            }
            
            lastSign = currentSign;
        }
        
        return randomVel;
    }
}