using System.Collections.Generic;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.Application.Interfaces.DAL
{
    public interface IVideoRepository
    {
        List<VideoRecord> GetAllVideos();
    }
}