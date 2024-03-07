namespace VisualCfg.Logic;

public class Either<Left, Right>
{
    public Left Result { get; }
    public Right Error { get; }

    public Either(Left result) => Result = result;

    public Either(Right error) => Error = error;

    public bool Success => Result is not null;
}
