# Images Folder

## Hướng dẫn thêm ảnh nền cho trang đăng nhập

### Bước 1: Chuẩn bị ảnh
- Đặt ảnh nền vào thư mục này với tên: `btec-fpt-background.jpg`
- Khuyến nghị:
  - Kích thước: 1920x1080px hoặc lớn hơn
  - Định dạng: JPG, PNG
  - Dung lượng: < 2MB (để tải nhanh)
  - Nội dung: Ảnh về trường học, giáo dục, hoặc logo BTEC FPT

### Bước 2: Đặt tên file
Đảm bảo tên file chính xác: `btec-fpt-background.jpg`

### Bước 3: Kiểm tra
Sau khi thêm ảnh, refresh trang đăng nhập để xem kết quả.

### Lưu ý:
- Nếu không có ảnh, trang sẽ hiển thị overlay gradient màu tối
- Có thể điều chỉnh độ mờ overlay trong file `Login.cshtml` (dòng 36-37)
- Để thay đổi tên file ảnh, sửa đường dẫn trong CSS: `url('/images/ten-file-cua-ban.jpg')`

