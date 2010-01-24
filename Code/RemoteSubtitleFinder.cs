﻿using System.Collections.Generic;
using MediaBrowser.Library.Entities;

namespace SubtitleProvider
{
    public class RemoteSubtitleFinder
    {
        #region Private Members

        private readonly Video video;

        private readonly List<SubtitleSourceBase> sources;

        #endregion

        #region Constructors

        public RemoteSubtitleFinder(Video video)
        {
            this.video = video;

            if (sources == null)
            {
                sources = new List<SubtitleSourceBase>();
            }

            sources.Add(new OpenSubtitlesSubtitleSource());
            sources.Add(new SubtitleSource());
        }

        #endregion

        #region Public Methods

        public Subtitle FindSubtitle(List<string> languages)
        {

            foreach (var source in sources)
            {
                var subtitle = source.FindSubtitleForVideo(video, languages);
                if (subtitle != null)
                {
                    return subtitle;
                }
            }

            return null;

        }

        #endregion

    }
}