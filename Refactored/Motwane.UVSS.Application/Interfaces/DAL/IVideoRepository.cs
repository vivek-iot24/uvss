using System.Collections.Generic;
using Motwane_UVSS.Domain.Entities;

namespace Motwane_UVSS.Application.Interfaces.DAL
{
    public interface IVideoRepository
    {
        List<VideoRecord> GetAllVideos();
    }
}