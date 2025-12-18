using System.Collections.Generic;
using System.Threading.Tasks;

public interface IResultsDataSource
{
    // 현재 웨이브 종료 시점의 결과(로컬/네트워크)를 가져온다.
    Task<List<PlayerResult>> GetWaveResultsAsync(int waveIndex, int teamWallHpLeft);

    // 닉네임별 누적 클리어 횟수 조회(DB 예정)
    Task<int> GetTotalClearCountAsync(string nickname);
}