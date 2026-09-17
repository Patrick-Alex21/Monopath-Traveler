public static class BattleInputActions
{
    private static PlayerInputAction _actions;
    public static PlayerInputAction Actions => _actions ??= new PlayerInputAction();
}