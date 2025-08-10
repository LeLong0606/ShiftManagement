using System.Threading;
using System.Threading.Tasks;

namespace ShiftManagement.Services
{
    public interface ISamsDataMigrator
    {
        // overwrite = true: cho phép cập nhật record đã tồn tại theo key; false: chỉ insert nếu chưa có
        Task RunAsync(bool overwrite = false, CancellationToken ct = default);
    }
}