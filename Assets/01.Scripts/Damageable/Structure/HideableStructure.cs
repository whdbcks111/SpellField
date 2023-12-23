using UnityEngine;

public class HideableStructure : Structure
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Player player))
        {
            player.AddState(PlayerState.Invisible, 0.1f);
        }
    }
}
