using YoutubeManager.Attributes;

namespace YoutubeManager.Enums
{
    public enum MenuAction
    {
        [Name("Thêm")]
        [ResourceImage(nameof(Resource.plus_16))]
        Add,

        [Name("Sửa")]
        [ResourceImage(nameof(Resource.edit_16))]
        Edit,

        [Name("Xóa")]
        [ResourceImage(nameof(Resource.Erase_16))]
        Delete,

        [Name("Mở trên trình duyệt")]
        [ResourceImage(nameof(Resource.old_edit_redo_16))]
        Open,

        [Name("Cột")]
        Column,

        CustomColumn,

        [Name("Kiểm tra tất cả kênh trong nhóm")]
        [ResourceImage(nameof(Resource.refresh02_16))]
        CheckChannelInGroup,

        [Name("Kiểm tra tất cả kênh")]
        [ResourceImage(nameof(Resource.refresh02_16))]
        CheckAllChannel,

        [Name("Sao chép địa chỉ kênh")]
        [ResourceImage(nameof(Resource.page_copy_16))]
        CopyAddress,

        [Name("Chuyển lên trên cùng")]
        [ResourceImage(nameof(Resource.control_double_090_16))]
        MoveToTop,

        [Name("Chuyển xuống dưới cùng")]
        [ResourceImage(nameof(Resource.control_double_down_090_16))]
        MoveToBottom,

        [Name("Xóa toàn bộ kênh chết")]
        [ResourceImage(nameof(Resource.Erase_16))]
        DeleteAllDead,

        [Name("Xóa toàn bộ kênh chết trong nhóm")]
        [ResourceImage(nameof(Resource.Erase_16))]
        DeleteAllDeadInGroup,

        [Name("Kiểm tra kênh đã chọn")]
        [ResourceImage(nameof(Resource.refresh02_16))]
        CheckChannelSelected,

        [Name("Xuất txt")]
        [ResourceImage(nameof(Resource.txt_file_16))]
        ExportTxt,
    }
}
