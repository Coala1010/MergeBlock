//
//  APDInterstital.h
//
//  AppodealSDK version 2.5.15
//
//  Copyright © 2020 Appodeal, Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <Appodeal/APDSdk.h>

#ifdef ADVANCED_INTEGRATION
#import <Appodeal/AppodealRequestDelegateProtocol.h>
#endif


@class APDInterstitialAd;
/**
 Declaration of interstitial delegate
 */
@protocol APDInterstitalAdDelegate <NSObject>

@optional
/**
 Method called if precache interstitial (cheap and fast loading) did load.
 @param interstitialAd Instance of ready interstitial
 @param isPrecache Boolean flag that indicates that loaded interstitial is preache or not
 */
- (void)interstitialAdDidLoad:(nonnull APDInterstitialAd *)interstitialAd isPrecache:(BOOL)isPrecache;
/**
 Method called if interstitial mediation attempt was unsuccessful
 @param interstitialAd Instance of failed interstitial
 @param error Mediation error
 */
- (void)interstitialAd:(nonnull APDInterstitialAd *)interstitialAd didFailToLoadWithError:(nonnull NSError *)error;
/**
 Method called when interstitial shows on screen
 @param interstitialAd interstitialAd Shown interstitial
 */
- (void)interstitialAdDidAppear:(nonnull APDInterstitialAd *)interstitialAd;
/**
 Method called when interstitial did dismiss from screen
 @param interstitialAd Shown interstitial
 */
- (void)interstitialAdDidDisappear:(nonnull APDInterstitialAd *)interstitialAd;
/**
 Method called in case that interstitial failed while showing
 For example an error occurs in current ad network
 @param interstitialAd Shown interstitial
 @param error Error
 */
- (void)interstitialAd:(nonnull APDInterstitialAd *)interstitialAd didFailToPresentWithError:(nonnull NSError *)error;
/**
 Call when user taps on interstitial
 @param interstitialAd Shown interstitial
 */
- (void)interstitialAdDidRecieveTapAction:(nonnull APDInterstitialAd *)interstitialAd;
/**
 Called if interstitial ad expired
 @param interstitialAd Interstitial ad instance
 */
- (void)interstitialAdDidExpire:(nonnull APDInterstitialAd *)interstitialAd;

@end
/**
 You should have strong reference on the instance of loading an interstitial
 Instance of interstitial ad can try to load ad only once!
 Create new interstitial before any call -loadAd!
 @note
 <pre> - (void) loadInterstitial {
 self.interstital = [APDInterstitialAd new];
 self.interstital.delegate = self;
 [self.interstital loadAd]
 }
 </pre>
 */
@interface APDInterstitialAd : NSObject
/**
 Set interstitial delegate
 */
@property (weak, nonatomic, nullable) id<APDInterstitalAdDelegate> delegate;
/**
 Set custom SDK
 */
@property (weak, nonatomic, nullable) APDSdk *customSdk;
/**
 Get interstitial availability
 */
@property (assign, nonatomic, readonly, getter=isReady) BOOL ready;
/**
 Set autocache
 */
@property (assign, nonatomic) BOOL autocache;
/**
  Get interstitial already shown
 */
@property (assign, nonatomic, readonly) BOOL hasBeenPresented;
/**
 Initializator
 @param sdk Custom SDK
 @param delegate Interstitial delegate
 @param autocache Autocache
 @return Instance of APDInterstitialAd
 */
+ (nonnull instancetype)interstitialWithSdk:(nullable APDSdk *)sdk
                                   delegate:(nullable id<APDInterstitalAdDelegate>)delegate
                                  autocache:(BOOL)autocache;
/**
 Start loading interstitial
 */
- (void)loadAd;
/**
 Show ready interstitial from view controller
 @param viewController Current presented view controller
 @param placement Current placement
 */
- (void)presentFromViewController:(nonnull UIViewController *)viewController placement:(nonnull NSString *)placement;

@end

