﻿using System.Collections.Generic;
using System.Threading;

namespace LLama.Abstractions
{
    /// <summary>
    /// A high level interface for LLama models.
    /// </summary>
    public interface ILLamaExecutor
    {
        /// <summary>
        /// The loaded context for this executor.
        /// </summary>
        public LLamaContext Context { get; }
        
        // LLava Section
        public bool IsMultiModal { get; }
        public bool MultiModalProject { get;  }
        public LLavaWeights? ClipModel { get;  }        
        public string ImagePath { get; set; }
        
        
        /// <summary>
        /// Asynchronously infers a response from the model.
        /// </summary>
        /// <param name="text">Your prompt</param>
        /// <param name="inferenceParams">Any additional parameters</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns></returns>
        IAsyncEnumerable<string> InferAsync(string text, IInferenceParams? inferenceParams = null, CancellationToken token = default);
    }
}
