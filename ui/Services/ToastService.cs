namespace ui.Services;

public class ToastService
{
    public event Action? Update;
    private List<Toast> _currentToasts = new List<Toast>();

    public List<Toast> GetToasts()
    {
        return _currentToasts;
    }

    public void AddSuccess(string message)
    {
        _currentToasts.Add(new Toast(message));
        OnUpdate();
    }
    
    public void AddError(string message)
    {
        _currentToasts.Add(new Toast(message, "error"));
        OnUpdate();
    }
    
    public void AddError(Exception e)
    {
        _currentToasts.Add(new Toast(e.Message, "error"));
        OnUpdate();
    }
    
    public void RemoveToast(Guid id)
    {
        _currentToasts = _currentToasts.Where(x => x.Id != id).ToList();
        OnUpdate();
    }

    private void OnUpdate()
    {
        Update?.Invoke();
    }
}

public struct Toast
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; }
    public string Kind { get; set; }

    public string Classes
    {
        get
        {
            if (Kind == "success")
            {
                return "bg-success text-white";
            }
            else
            {
                return "bg-danger text-white";
            }
        }
    }

    public Toast(string message, string kind = "success")
    {
        Message = message;
        Kind = kind;
    }
}