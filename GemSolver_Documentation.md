# Gem Solver - Tài liệu Đặc tả Thuật toán và Hướng dẫn Sử dụng

## 1. Tổng quan

Gem Solver là một thuật toán tìm kiếm giải pháp cho bài toán ghép đôi gem trong game puzzle. Thuật toán sử dụng kỹ thuật **Depth-Limited DFS** với IDA* (Iterative Deepening A*) để tìm các nước đi hợp lệ ghép các gem có cùng giá trị và đường đi (ngang, dọc, hoặc chéo).

---

## 2. Định nghĩa Bài toán

### 2.1. Input
- Chuỗi các chữ số (0-9) đại diện cho giá trị các gem trên bảng
- Kích thước bảng: 9 cột (cố định)
- Số hàng = độ dài chuỗi / 9

### 2.2. Output
- Danh sách các nước đi (match moves) để ghép hết các gem cần thiết
- Mỗi nước đi có dạng: `rowA,colA,rowB,colB`

### 2.3. Mục tiêu
- Ghép tất cả các gem có giá trị = 5 (gem đặc biệt)
- Số gem cần ghép = `floor(TotalGem5 / 2) * 2`

---

## 3. Quy tắc Ghép đôi (BoardRules)

### 3.1. Quy tắc giá trị khớp (`IsMatchValue`)
```csharp
a == b || (a + b == 10)
```
- Hai gem khớp nếu có cùng giá trị, HOẶC
- Tổng hai giá trị bằng 10

**Ví dụ:**
- 2 ↔ 8 ✓
- 5 ↔ 5 ✓
- 1 ↔ 9 ✓

### 3.2. Quy tắc đường đi (`IsPathClear`)
Hai gem chỉ có thể ghép khi đường đi giữa chúng là đường thẳng:
- **Ngang**: Cùng hàng, cột khác nhau
- **Dọc**: Cùng cột, hàng khác nhau  
- **Chéo**: Hiệu hàng = Hiệu cột (đường chéo 45°)

Đường đi không được có gem nào khác chắn ở giữa.

---

## 4. Thuật toán Chi tiết

### 4.1. Cấu trúc Dữ liệu

```csharp
// filepath: Assets/Scripts/Optional/MatchMove.cs
public struct MatchMove
{
    public int A;  // Index gem thứ nhất
    public int B;  // Index gem thứ hai
}
```

```csharp
// filepath: Assets/Scripts/Optional/GemSolveResult.cs
public class GemSolveResult
{
    public GemSolveStatus Status;        // Solved / NoSolution
    public List<List<MatchMove>> Solutions;
    public int RequiredGemCount;         // Số gem cần ghép
    public int TotalGemCount;            // Tổng số gem
    public int BestMoveCount;            // Số nước tối thiểu
}
```

### 4.2. Luồng Thuật toán

```
Solve(input)
    │
    ├─► Init: Parse input, đếm gem giá trị 5
    │
    ├─► Tính lower bound = ceil(requiredGemCount / 2)
    │
    ├─► For depthLimit = lowerBound → maxDepth
    │       │
    │       ├─► Clear state
    │       │
    │       ├─► DepthLimitedDFS()
    │       │       │
    │       │       ├─► Kiểm tra đã đủ gem chưa
    │       │       ├─► Kiểm tra bound (còn đủ depth không)
    │       │       ├─► Memoization: stateKey → bestRemainingDepth
    │       │       ├─► Lấy tất cả legal moves
    │       │       ├─► Sắp xếp theo score (ưu tiên gem 5, khoảng cách ngắn)
    │       │       └─► Thử từng move → DFS đệ quy
    │       │
    │       └─► Nếu tìm được solution → return
    │
    └─► Không tìm được → return NoSolution
```

### 4.3. Tối ưu hóa

| Kỹ thuật | Mô tả |
|----------|-------|
| **Iterative Deepening** | Tăng dần độ sâu tìm kiếm từ lower bound |
| **Memoization** | Tránh duyệt lại state đã visited với depth thấp hơn |
| **Move Ordering** | Ưu tiên moves có giá trị cao |
| **Pruning** | Cắt nhánh khi không thể đạt target |
| **Max Solutions** | Giới hạn 10 solutions |

---

## 5. Định dạng Input/Output

### 5.1. Input (`input.txt`)

```
461823292118551564897258632132642316974542558239279633
```

- Là chuỗi các chữ số liên tiếp không có khoảng trắng
- Mỗi chữ số = giá trị của một gem
- Thứ tự: đọc từ trái sang phải, từ trên xuống dưới

### 5.2. Output (`output.txt`)

```
1,3,1,4|4,6,4,7|3,3,3,4|2,5,3,5|1,6,4,3
1,3,1,4|4,6,4,7|3,3,3,4|0,3,2,5|1,6,4,3
...
```

- Mỗi dòng = một giải pháp
- Dùng `|` để phân cách các nước đi
- Mỗi nước đi: `rowA,colA,rowB,colB`
- Nếu không có giải pháp: `NO_SOLUTION`

---

## 6. Hướng dẫn Sử dụng

### 6.1. Cách chạy Solver

1. Mở Unity Editor
2. Đặt file input vào thư mục `Assets/input.txt`
3. Vào menu: **Tools → Gem Solver → Run Solver**
4. Kết quả xuất ra `Assets/output.txt`
5. Xem log trong Unity Console

### 6.2. Cách tạo Input
- Liệt kê giá trị từng gem theo thứ tự hàng ngang
- Ghép thành chuỗi liên tiếp không có khoảng trắng

**Ví dụ:** Bảng 3x3 với các giá trị:
```
1 2 3
4 5 6
7 8 9
```
→ Input: `123456789`

### 6.3. Giải thích Output

```
1,3,1,4|4,6,4,7|3,3,3,4|2,5,3,5|1,6,4,3
```

| Nước đi | Gem A | Gem B |
|---------|-------|-------|
| 1,3,1,4 | (1,3) | (1,4) |
| 4,6,4,7 | (4,6) | (4,7) |
| 3,3,3,4 | (3,3) | (3,4) |
| 2,5,3,5 | (2,5) | (3,5) |
| 1,6,4,3 | (1,6) | (4,3) |
---

## 7. Giới hạn và Lưu ý

- **Kích thước cố định**: 9 cột
- **Giới hạn solutions**: Tối đa 10 giải pháp
- **Không đảm bảo optimal**: Thuật toán tìm giải pháp hợp lệ, không nhất thiết là tối ưu nhất
- **Performance**: Với bảng lớn (>100 gems), thời gian có thể rất lâu do brute-force search