# NumMatch - Game Puzzle Ghép Số

> Một game puzzle ghép số được xây dựng trên Unity với nhiều màn chơi khác nhau.

## 📱 Giới thiệu

**NumMatch** là một game puzzle giải trí nơi người chơi ghép các con số trên lưới. Game có nhiều chế độ chơi hấp dẫn.

## 🎮 Các cơ chế game

| Cơ chế | Mô tả |
|--------|-------|
| **Generage** | Khởi tạo các màn chơi và gem thỏa mãn các ràng buộc cho trước |
| **AddNum** | Sao chép toàn bộ số chưa được loại bỏ trên màn chơi và thêm vào bảng hiện tại |
| **Collapse** | Xóa hàng khi các số trên hàng đều đã được match |
| **Stage** | Hệ thống quản lý level tiến độ và số lượng gem cần thu thập trong level đó |

## 🏆 Logic Thắng/Thua

### Thắng (Chuyển Stage)
- Khi người chơi thu thập đủ số lượng gem theo yêu cầu của stage → Chuyển sang stage tiếp theo
- Khi clear cả 3 stage → Người chơi hoàn thành trò chơi và có thể chơi lại từ đầu

### Thua (Chơi lại)
- Khi không còn số nào có thể match trên bảng → Thua và chơi lại stage đó

## 🎯 Tính năng

- Nhiều cơ chế game (AddNum, Collapse, Gem, Fallback)
- Hiệu ứng âm thanh
- Progression qua các stage/level
- Cơ chế xác định thắng/thua
- Hệ thống khởi tạo và quản lý bảng game

## 🛠️ Công nghệ sử dụng

- **Engine**: Unity 2021.3 LTS
- **Ngôn ngữ**: C#
- **Resolution**: 1080 x 1920 (Portrait)

## 📁 Cấu trúc dự án

```
Assets/
├── Scripts/
│   ├── AddNum/      # Cơ chế thêm số
│   ├── Audio/       # Quản lý âm thanh
│   ├── Board/       # Logic bảng game
│   ├── Cell/        # Hệ thống ô
│   ├── Collapse/    # Cơ chế thu hẹp bảng
│   ├── Fallback/    # Cơ chế dự phòng
│   ├── Gem/         # Thu thập ngọc
│   ├── Input/       # Xử lý input
│   ├── Manager/     # Các manager game
│   │   ├── AddManager.cs      # Quản lý cơ chế Add
│   │   ├── AudioManager.cs    # Quản lý âm thanh
│   │   ├── BoardManager.cs    # Quản lý bảng
│   │   ├── GameManager.cs     # Quản lý game tổng
│   │   └── StageManager.cs    # Quản lý stage/level
│   ├── UI/          # Component UI
│   ├── WinLoss/     # Cơ chế thắng/thua
│   └── ScriptableObject/  # Scriptable Objects
├── Prefabs/         # Game prefabs
├── Scenes/          # Các scene game
├── Sprites/         # Đồ họa
└── UI/              # Tài nguyên UI
```

## 🚀 Bắt đầu

### Yêu cầu
- Unity 2021.3 trở lên
- Visual Studio hoặc VS Code

### Cài đặt
1. Clone repository về máy
2. Mở `Pentoro.sln` trong Unity Hub
3. Đợi restore packages
4. Mở project và chạy thử