export const EXAMPLE_STYLES = `// PlayerCard.styles.cs — style helpers shared across the component
using UnityEngine.UIElements;

namespace MyGame.UI
{
    public static class PlayerCardStyles
    {
        public static readonly StyleColor HealthGreen = new(new Color(0.2f, 0.8f, 0.3f));
        public static readonly StyleColor DamageRed = new(new Color(0.9f, 0.2f, 0.2f));
        public static readonly StyleLength AvatarSize = new(64);

        public static void ApplyCardLayout(VisualElement el)
        {
            el.style.flexDirection = FlexDirection.Row;
            el.style.paddingLeft = 8;
            el.style.paddingRight = 8;
            el.style.paddingTop = 4;
            el.style.paddingBottom = 4;
        }
    }
}`

export const EXAMPLE_TYPES = `// PlayerCard.types.cs — shared type definitions
namespace MyGame.UI
{
    public enum PlayerRank { Bronze, Silver, Gold, Diamond }

    public readonly struct PlayerInfo
    {
        public string Name { get; init; }
        public int Health { get; init; }
        public int MaxHealth { get; init; }
        public PlayerRank Rank { get; init; }
    }
}`

export const EXAMPLE_UTILS = `// PlayerCard.utils.cs — reusable helpers
namespace MyGame.UI
{
    public static class PlayerCardUtils
    {
        public static string FormatHealth(int current, int max)
            => $"{current} / {max} HP";

        public static string RankLabel(PlayerRank rank) => rank switch
        {
            PlayerRank.Diamond => "★ Diamond",
            PlayerRank.Gold    => "● Gold",
            PlayerRank.Silver  => "○ Silver",
            _                  => "· Bronze",
        };
    }
}`

export const EXAMPLE_UITKX = `@namespace MyGame.UI
@using UnityEngine.UIElements

component PlayerCard {
  @props { PlayerInfo player }

  var healthColor = player.Health > player.MaxHealth / 2
    ? PlayerCardStyles.HealthGreen
    : PlayerCardStyles.DamageRed;

  return (
    <VisualElement>
      <Text text={player.Name} />
      <Text text={PlayerCardUtils.FormatHealth(player.Health, player.MaxHealth)}
            style:color={healthColor} />
      <Text text={PlayerCardUtils.RankLabel(player.Rank)} />
    </VisualElement>
  );
}`

export const EXAMPLE_DIRECTORY = `Assets/
  UI/
    PlayerCard/
      PlayerCard.uitkx          ← component markup
      PlayerCard.styles.cs      ← optional: style constants & helpers
      PlayerCard.types.cs       ← optional: enums, structs, DTOs
      PlayerCard.utils.cs       ← optional: pure helper functions`
